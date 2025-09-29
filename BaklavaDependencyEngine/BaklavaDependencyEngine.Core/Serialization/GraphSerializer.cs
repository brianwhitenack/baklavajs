using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using BaklavaDependencyEngine.Core.Models;

namespace BaklavaDependencyEngine.Core.Serialization
{
    public class GraphSerializer
    {
        private readonly Dictionary<string, Type> _nodeTypes = new Dictionary<string, Type>();

        public void RegisterNodeType<T>(string typeName) where T : Node
        {
            _nodeTypes[typeName] = typeof(T);
        }

        public void RegisterNodeType(string typeName, Type nodeType)
        {
            if (!typeof(Node).IsAssignableFrom(nodeType))
                throw new ArgumentException($"Type {nodeType} must inherit from Node");

            _nodeTypes[typeName] = nodeType;
        }

        public string SerializeGraph(Graph graph, bool wrapInGraphFile = true)
        {
            GraphData graphData = new GraphData
            {
                Id = graph.Id,
                Nodes = graph.Nodes.Select(SerializeNode).ToList(),
                Connections = graph.Connections.Select(SerializeConnection).ToList()
            };

            if (wrapInGraphFile)
            {
                var graphFile = new GraphFile(graphData);
                return JsonConvert.SerializeObject(graphFile, Formatting.Indented);
            }

            return JsonConvert.SerializeObject(graphData, Formatting.Indented);
        }

        public Graph DeserializeGraph(string json)
        {
            GraphData? graphData = null;

            // Try to deserialize as GraphFile first (TypeScript format)
            try
            {
                var graphFile = JsonConvert.DeserializeObject<GraphFile>(json);
                if (graphFile?.Graph != null)
                {
                    graphData = graphFile.Graph;
                }
            }
            catch
            {
                // If that fails, try direct GraphData format
                graphData = JsonConvert.DeserializeObject<GraphData>(json);
            }

            if (graphData == null)
            {
                throw new InvalidOperationException("Unable to deserialize graph data");
            }

            Graph graph = new Graph { Id = graphData.Id };

            // Create nodes
            Dictionary<string, Node>? nodeMap = new Dictionary<string, Node>();
            Dictionary<string, NodeInterface>? interfaceMap = new Dictionary<string, NodeInterface>();

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
                    Connection? connection = new Connection(from, to) { Id = connData.Id };
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
                Type = node.Type,
                Title = node.Title,
                Inputs = node.Inputs.ToDictionary(kvp => kvp.Key, kvp => SerializeInterface(kvp.Value)),
                Outputs = node.Outputs.ToDictionary(kvp => kvp.Key, kvp => SerializeInterface(kvp.Value))
            };
        }

        private Node DeserializeNode(NodeData nodeData)
        {
            if (!_nodeTypes.TryGetValue(nodeData.Type, out var nodeType))
            {
                throw new InvalidOperationException($"Unknown node type: {nodeData.Type}");
            }

            Node? node = (Node)Activator.CreateInstance(nodeType);
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
    }

    // Wrapper class to match TypeScript format
    public class GraphFile
    {
        public GraphData Graph { get; set; }

        public GraphFile()
        {
            Graph = new GraphData();
        }

        public GraphFile(GraphData graph)
        {
            Graph = graph;
        }
    }

    // Data classes for serialization
    public class GraphData
    {
        public string Id { get; set; }
        public List<NodeData> Nodes { get; set; }
        public List<ConnectionData> Connections { get; set; }
    }

    public class NodeData
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public Dictionary<string, InterfaceData> Inputs { get; set; }
        public Dictionary<string, InterfaceData> Outputs { get; set; }
    }

    public class InterfaceData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public object Value { get; set; }
        public bool AllowMultipleConnections { get; set; }
        public bool IsInput { get; set; }
    }

    public class ConnectionData
    {
        public string Id { get; set; }
        public string FromId { get; set; }
        public string ToId { get; set; }
    }
}