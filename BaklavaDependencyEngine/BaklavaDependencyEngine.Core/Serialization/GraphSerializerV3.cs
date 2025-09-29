using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using BaklavaDependencyEngine.Core.Models;

namespace BaklavaDependencyEngine.Core.Serialization
{
    /// <summary>
    /// GraphSerializer V3 using JsonSubTypes for automatic polymorphic serialization
    /// No manual registration needed - all node types are handled via attributes on the Node base class
    /// </summary>
    public class GraphSerializerV3
    {
        private readonly JsonSerializerSettings _settings;

        public GraphSerializerV3()
        {
            _settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.None,
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Include
            };
        }

        public string SerializeGraph(Graph graph, bool wrapInGraphFile = true)
        {
            var graphData = new GraphDataV3
            {
                Id = graph.Id,
                Nodes = graph.Nodes,  // Direct serialization - JsonSubTypes handles the polymorphism
                Connections = graph.Connections.Select(SerializeConnection).ToList()
            };

            if (wrapInGraphFile)
            {
                // Need to convert to old GraphData for wrapper compatibility
                var oldGraphData = new GraphData
                {
                    Id = graphData.Id,
                    Nodes = graphData.Nodes.Select(SerializeNodeToOldFormat).ToList(),
                    Connections = graphData.Connections
                };
                var graphFile = new GraphFile(oldGraphData);
                return JsonConvert.SerializeObject(graphFile, _settings);
            }

            return JsonConvert.SerializeObject(graphData, _settings);
        }

        private NodeData SerializeNodeToOldFormat(Node node)
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

        public Graph DeserializeGraph(string json)
        {
            GraphDataV3? graphData = null;

            // Try to deserialize as GraphFile first (TypeScript format)
            try
            {
                // For compatibility, we need to handle the old format
                var jObject = Newtonsoft.Json.Linq.JObject.Parse(json);
                if (jObject["Graph"] != null)
                {
                    // It's wrapped in GraphFile, extract the Graph part
                    var graphJson = jObject["Graph"].ToString();
                    graphData = JsonConvert.DeserializeObject<GraphDataV3>(graphJson, _settings);
                }
            }
            catch
            {
                // If that fails, try direct GraphDataV3 format
                graphData = JsonConvert.DeserializeObject<GraphDataV3>(json, _settings);
            }

            if (graphData == null)
            {
                throw new InvalidOperationException("Unable to deserialize graph data");
            }

            var graph = new Graph { Id = graphData.Id };

            // Create interface map for connections
            var interfaceMap = new Dictionary<string, NodeInterface>();

            foreach (var node in graphData.Nodes)
            {
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

    /// <summary>
    /// Simplified GraphDataV3 that directly uses Node objects
    /// JsonSubTypes handles the polymorphic serialization
    /// </summary>
    public class GraphDataV3
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public List<Node> Nodes { get; set; } = new List<Node>();
        public List<ConnectionData> Connections { get; set; } = new List<ConnectionData>();
    }
}