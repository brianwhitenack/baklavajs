using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using BaklavaDependencyEngine.Core.Models;

namespace BaklavaDependencyEngine.Core.Serialization
{
    /// <summary>
    /// GraphSerializer V2 that automatically discovers and deserializes node types
    /// without manual registration using reflection and type discriminators
    /// </summary>
    public class GraphSerializerV2
    {
        private readonly JsonSerializerSettings _settings;
        private readonly Dictionary<string, Type> _nodeTypeCache;

        public GraphSerializerV2()
        {
            _nodeTypeCache = DiscoverNodeTypes();

            _settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.None,
                Converters = new List<JsonConverter>
                {
                    new NodeDataConverter(_nodeTypeCache)
                },
                Formatting = Formatting.Indented
            };
        }

        /// <summary>
        /// Discovers all types that inherit from Node in the current assembly
        /// and referenced assemblies
        /// </summary>
        private Dictionary<string, Type> DiscoverNodeTypes()
        {
            var nodeTypes = new Dictionary<string, Type>();

            // Get all assemblies
            var assemblies = new List<Assembly>
            {
                typeof(Node).Assembly // Core assembly
            };

            // Add entry assembly if different
            var entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly != null && !assemblies.Contains(entryAssembly))
            {
                assemblies.Add(entryAssembly);
            }

            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes()
                        .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(Node)));

                    foreach (var type in types)
                    {
                        // Use the class name as the type discriminator
                        var typeName = type.Name;

                        if (!nodeTypes.ContainsKey(typeName))
                        {
                            nodeTypes[typeName] = type;
                            Console.WriteLine($"Discovered node type: {typeName} -> {type.FullName}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Could not load types from assembly {assembly.FullName}: {ex.Message}");
                }
            }

            return nodeTypes;
        }

        public string SerializeGraph(Graph graph, bool wrapInGraphFile = true)
        {
            var graphData = new GraphData
            {
                Id = graph.Id,
                Nodes = graph.Nodes.Select(SerializeNode).ToList(),
                Connections = graph.Connections.Select(SerializeConnection).ToList()
            };

            if (wrapInGraphFile)
            {
                var graphFile = new GraphFile(graphData);
                return JsonConvert.SerializeObject(graphFile, _settings);
            }

            return JsonConvert.SerializeObject(graphData, _settings);
        }

        public Graph DeserializeGraph(string json)
        {
            GraphData? graphData = null;

            // Try to deserialize as GraphFile first (TypeScript format)
            try
            {
                var graphFile = JsonConvert.DeserializeObject<GraphFile>(json, _settings);
                if (graphFile?.Graph != null)
                {
                    graphData = graphFile.Graph;
                }
            }
            catch
            {
                // If that fails, try direct GraphData format
                graphData = JsonConvert.DeserializeObject<GraphData>(json, _settings);
            }

            if (graphData == null)
            {
                throw new InvalidOperationException("Unable to deserialize graph data");
            }

            var graph = new Graph { Id = graphData.Id };

            // Create nodes
            var nodeMap = new Dictionary<string, Node>();
            var interfaceMap = new Dictionary<string, NodeInterface>();

            foreach (var nodeData in graphData.Nodes)
            {
                var node = DeserializeNode(nodeData);
                nodeMap[node.Id] = node;
                graph.AddNode(node);

                // Map interfaces
                foreach (var input in node.Inputs.Values)
                    interfaceMap[input.Id] = input;
                foreach (var output in node.Outputs.Values)
                    interfaceMap[output.Id] = output;
            }

            // Create connections
            foreach (var connData in graphData.Connections)
            {
                if (interfaceMap.TryGetValue(connData.FromId, out var from) &&
                    interfaceMap.TryGetValue(connData.ToId, out var to))
                {
                    var connection = new Connection(from, to) { Id = connData.Id };
                    graph.AddConnection(connection);
                }
            }

            return graph;
        }

        private NodeData SerializeNode(Node node)
        {
            return new NodeData
            {
                Id = node.Id,
                Type = node.GetType().Name, // Use actual class name as type
                Title = node.Title,
                Inputs = node.Inputs.ToDictionary(kvp => kvp.Key, kvp => SerializeInterface(kvp.Value)),
                Outputs = node.Outputs.ToDictionary(kvp => kvp.Key, kvp => SerializeInterface(kvp.Value))
            };
        }

        private Node DeserializeNode(NodeData nodeData)
        {
            if (!_nodeTypeCache.TryGetValue(nodeData.Type, out var nodeType))
            {
                // Try to find the type dynamically if not in cache
                nodeType = FindNodeType(nodeData.Type);

                if (nodeType == null)
                {
                    throw new InvalidOperationException($"Unknown node type: {nodeData.Type}. Available types: {string.Join(", ", _nodeTypeCache.Keys)}");
                }

                // Add to cache for next time
                _nodeTypeCache[nodeData.Type] = nodeType;
            }

            var node = (Node)Activator.CreateInstance(nodeType)!;
            node.Id = nodeData.Id;
            node.Title = nodeData.Title;

            // Update interface values
            foreach (var inputData in nodeData.Inputs)
            {
                if (node.Inputs.TryGetValue(inputData.Key, out var input))
                {
                    input.Id = inputData.Value.Id;
                    input.Value = inputData.Value.Value;
                    input.AllowMultipleConnections = inputData.Value.AllowMultipleConnections;
                }
            }

            foreach (var outputData in nodeData.Outputs)
            {
                if (node.Outputs.TryGetValue(outputData.Key, out var output))
                {
                    output.Id = outputData.Value.Id;
                    output.Value = outputData.Value.Value;
                }
            }

            return node;
        }

        private Type? FindNodeType(string typeName)
        {
            // Search all loaded assemblies for the type
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var type = assembly.GetTypes()
                        .FirstOrDefault(t => t.Name == typeName && t.IsSubclassOf(typeof(Node)));

                    if (type != null)
                        return type;
                }
                catch
                {
                    // Skip assemblies that can't be loaded
                }
            }

            return null;
        }

        private InterfaceData SerializeInterface(NodeInterface nodeInterface)
        {
            return new InterfaceData
            {
                Id = nodeInterface.Id,
                Name = nodeInterface.Name,
                Value = nodeInterface.Value,
                AllowMultipleConnections = nodeInterface.AllowMultipleConnections,
                IsInput = nodeInterface.IsInput
            };
        }

        private ConnectionData SerializeConnection(Connection connection)
        {
            return new ConnectionData
            {
                Id = connection.Id,
                FromId = connection.From.Id,
                ToId = connection.To.Id
            };
        }

        /// <summary>
        /// Custom JSON converter for NodeData that preserves unknown properties
        /// </summary>
        private class NodeDataConverter : JsonConverter<NodeData>
        {
            private readonly Dictionary<string, Type> _nodeTypes;

            public NodeDataConverter(Dictionary<string, Type> nodeTypes)
            {
                _nodeTypes = nodeTypes;
            }

            public override void WriteJson(JsonWriter writer, NodeData? value, JsonSerializer serializer)
            {
                if (value == null)
                {
                    writer.WriteNull();
                    return;
                }

                // Write manually to avoid circular reference
                writer.WriteStartObject();

                writer.WritePropertyName("Id");
                writer.WriteValue(value.Id);

                writer.WritePropertyName("Type");
                writer.WriteValue(value.Type);

                writer.WritePropertyName("Title");
                writer.WriteValue(value.Title);

                writer.WritePropertyName("Inputs");
                writer.WriteStartObject();
                foreach (var input in value.Inputs)
                {
                    writer.WritePropertyName(input.Key);
                    serializer.Serialize(writer, input.Value);
                }
                writer.WriteEndObject();

                writer.WritePropertyName("Outputs");
                writer.WriteStartObject();
                foreach (var output in value.Outputs)
                {
                    writer.WritePropertyName(output.Key);
                    serializer.Serialize(writer, output.Value);
                }
                writer.WriteEndObject();

                writer.WriteEndObject();
            }

            public override NodeData? ReadJson(JsonReader reader, Type objectType, NodeData? existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                var token = JToken.Load(reader);
                var nodeData = new NodeData();

                // Manually deserialize to preserve all properties
                nodeData.Id = token["Id"]?.Value<string>() ?? Guid.NewGuid().ToString();
                nodeData.Type = token["Type"]?.Value<string>() ?? "UnknownNode";
                nodeData.Title = token["Title"]?.Value<string>() ?? nodeData.Type;

                // Deserialize inputs and outputs
                nodeData.Inputs = new Dictionary<string, InterfaceData>();
                var inputs = token["Inputs"] as JObject;
                if (inputs != null)
                {
                    foreach (var prop in inputs.Properties())
                    {
                        var interfaceData = prop.Value.ToObject<InterfaceData>(serializer);
                        if (interfaceData != null)
                            nodeData.Inputs[prop.Name] = interfaceData;
                    }
                }

                nodeData.Outputs = new Dictionary<string, InterfaceData>();
                var outputs = token["Outputs"] as JObject;
                if (outputs != null)
                {
                    foreach (var prop in outputs.Properties())
                    {
                        var interfaceData = prop.Value.ToObject<InterfaceData>(serializer);
                        if (interfaceData != null)
                            nodeData.Outputs[prop.Name] = interfaceData;
                    }
                }

                return nodeData;
            }
        }
    }
}