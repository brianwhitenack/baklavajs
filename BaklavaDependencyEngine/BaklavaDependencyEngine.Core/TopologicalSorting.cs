using BaklavaDependencyEngine.Core.Models;

namespace BaklavaDependencyEngine.Core
{
    public class CycleException : Exception
    {
        public CycleException() : base("Cycle detected in the graph")
        {
        }
    }

    public class TopologicalSortingResult
    {
        public List<Node> CalculationOrder { get; set; } = new List<Node>();
        public Dictionary<Node, List<Connection>> ConnectionsFromNode { get; set; } = new Dictionary<Node, List<Connection>>();
        public Dictionary<string, string> InterfaceIdToNodeId { get; set; } = new Dictionary<string, string>();
    }

    public static class TopologicalSorting
    {
        /// <summary>
        /// Uses Kahn's algorithm to topologically sort the nodes in the graph
        /// </summary>
        public static TopologicalSortingResult SortTopologically(Graph graph)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));

            return SortTopologically(graph.Nodes, graph.Connections);
        }

        /// <summary>
        /// Uses Kahn's algorithm to topologically sort the nodes
        /// </summary>
        public static TopologicalSortingResult SortTopologically(
            IReadOnlyList<Node> nodes,
            IReadOnlyList<Connection> connections)
        {
            var result = new TopologicalSortingResult();

            // Build InterfaceIdToNodeId mapping
            foreach (var node in nodes)
            {
                foreach (var input in node.Inputs.Values)
                {
                    result.InterfaceIdToNodeId[input.Id] = node.Id;
                }
                foreach (var output in node.Outputs.Values)
                {
                    result.InterfaceIdToNodeId[output.Id] = node.Id;
                }
            }

            // Build adjacency list and connections from node
            var adjacency = new Dictionary<string, HashSet<string>>();
            foreach (var node in nodes)
            {
                var connectionsFromCurrentNode = connections
                    .Where(c => c.FromInterface != null && result.InterfaceIdToNodeId.ContainsKey(c.FromInterface.Id) &&
                                result.InterfaceIdToNodeId[c.FromInterface.Id] == node.Id)
                    .ToList();

                var adjacentNodes = new HashSet<string>(
                    connectionsFromCurrentNode
                        .Where(c => result.InterfaceIdToNodeId.ContainsKey(c.ToInterface.Id))
                        .Select(c => result.InterfaceIdToNodeId[c.ToInterface.Id])
                );

                adjacency[node.Id] = adjacentNodes;
                result.ConnectionsFromNode[node] = connectionsFromCurrentNode;
            }

            // Find start nodes (nodes with no incoming connections)
            var startNodes = new List<Node>(nodes);
            foreach (var connection in connections)
            {
                if (result.InterfaceIdToNodeId.ContainsKey(connection.ToInterface.Id))
                {
                    var toNodeId = result.InterfaceIdToNodeId[connection.ToInterface.Id];
                    startNodes.RemoveAll(n => n.Id == toNodeId);
                }
            }

            // Perform topological sort
            var sorted = new List<Node>();
            while (startNodes.Count > 0)
            {
                var node = startNodes[startNodes.Count - 1];
                startNodes.RemoveAt(startNodes.Count - 1);
                sorted.Add(node);

                var nodesConnectedFromN = adjacency[node.Id];
                var toRemove = new List<string>(nodesConnectedFromN);

                foreach (var mId in toRemove)
                {
                    nodesConnectedFromN.Remove(mId);

                    // Check if node mId has no more incoming edges
                    if (adjacency.Values.All(connectedNodes => !connectedNodes.Contains(mId)))
                    {
                        var m = nodes.First(n => n.Id == mId);
                        startNodes.Add(m);
                    }
                }
            }

            // Check for cycles
            if (adjacency.Values.Any(c => c.Count > 0))
            {
                throw new CycleException();
            }

            result.CalculationOrder = sorted;
            return result;
        }

        /// <summary>
        /// Checks whether a graph contains a cycle
        /// </summary>
        public static bool ContainsCycle(Graph graph)
        {
            return ContainsCycle(graph.Nodes, graph.Connections);
        }

        /// <summary>
        /// Checks whether the provided set of nodes and connections contains a cycle
        /// </summary>
        public static bool ContainsCycle(IReadOnlyList<Node> nodes, IReadOnlyList<Connection> connections)
        {
            try
            {
                SortTopologically(nodes, connections);
                return false;
            }
            catch (CycleException)
            {
                return true;
            }
        }
    }
}