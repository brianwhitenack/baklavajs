using BaklavaDependencyEngine.Core.Models;

namespace BaklavaDependencyEngine.Core
{
    public class CalculationResult : Dictionary<string, Dictionary<string, object>>
    {
    }

    public class DependencyEngine
    {
        private readonly Dictionary<string, TopologicalSortingResult> _orderCache = new Dictionary<string, TopologicalSortingResult>();
        private bool _recalculateOrder = true;

        public event EventHandler<NodeCalculationEventArgs> BeforeNodeCalculation;
        public event EventHandler<NodeCalculationEventArgs> AfterNodeCalculation;

        public async Task<CalculationResult> RunGraph(Graph graph, object calculationData = null)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));

            var inputValues = GetInputValues(graph);
            return await RunGraph(graph, inputValues, calculationData);
        }

        public async Task<CalculationResult> RunGraph(
            Graph graph,
            Dictionary<string, object> inputs,
            object calculationData)
        {
            if (!_orderCache.ContainsKey(graph.Id) || _recalculateOrder)
            {
                _orderCache[graph.Id] = TopologicalSorting.SortTopologically(graph);
                _recalculateOrder = false;
            }

            var sortingResult = _orderCache[graph.Id];
            var result = new CalculationResult();

            foreach (var node in sortingResult.CalculationOrder)
            {
                // Gather inputs for this node
                var inputsForNode = new Dictionary<string, object>();
                foreach (var input in node.Inputs)
                {
                    inputsForNode[input.Key] = GetInterfaceValue(inputs, input.Value.Id);
                }

                // Fire before calculation event
                BeforeNodeCalculation?.Invoke(this, new NodeCalculationEventArgs
                {
                    Node = node,
                    Values = inputsForNode
                });

                // Calculate node outputs
                var context = new CalculationContext
                {
                    GlobalValues = calculationData,
                    Engine = this
                };

                var nodeOutputs = await node.Calculate(inputsForNode, context);

                // Validate outputs
                ValidateNodeCalculationOutput(node, nodeOutputs);

                // Fire after calculation event
                AfterNodeCalculation?.Invoke(this, new NodeCalculationEventArgs
                {
                    Node = node,
                    Values = nodeOutputs
                });

                // Store results
                result[node.Id] = nodeOutputs;

                // Transfer data to connected nodes
                if (sortingResult.ConnectionsFromNode.ContainsKey(node))
                {
                    foreach (var connection in sortingResult.ConnectionsFromNode[node])
                    {
                        // Find the output key for this connection
                        var outputKey = node.Outputs
                            .FirstOrDefault(kvp => kvp.Value.Id == connection.FromInterface.Id).Key;

                        if (outputKey == null)
                        {
                            throw new InvalidOperationException(
                                $"Could not find key for interface {connection.FromInterface.Id}");
                        }

                        var value = TransferData(nodeOutputs[outputKey], connection);

                        // Handle multiple connections
                        if (connection.ToInterface.AllowMultipleConnections)
                        {
                            if (inputs.ContainsKey(connection.ToInterface.Id))
                            {
                                var list = inputs[connection.ToInterface.Id] as List<object> ?? new List<object>();
                                list.Add(value);
                                inputs[connection.ToInterface.Id] = list;
                            }
                            else
                            {
                                inputs[connection.ToInterface.Id] = new List<object> { value };
                            }
                        }
                        else
                        {
                            inputs[connection.ToInterface.Id] = value;
                        }
                    }
                }
            }

            return result;
        }

        protected virtual object TransferData(object value, Connection connection)
        {
            // Hook point for data transformation during transfer
            // Can be overridden to implement type conversions
            return value;
        }

        private Dictionary<string, object> GetInputValues(Graph graph)
        {
            var inputValues = new Dictionary<string, object>();

            foreach (var node in graph.Nodes)
            {
                // Get values from unconnected inputs
                foreach (var input in node.Inputs.Values)
                {
                    if (input.ConnectionCount == 0)
                    {
                        inputValues[input.Id] = input.Value;
                    }
                }

                // For nodes without Calculate method, get output values
                // (This would be handled by checking if node has custom Calculate implementation)
                // For simplicity, we'll check if the node's Calculate method returns default outputs
                foreach (var output in node.Outputs.Values)
                {
                    if (!inputValues.ContainsKey(output.Id))
                    {
                        inputValues[output.Id] = output.Value;
                    }
                }
            }

            return inputValues;
        }

        private object GetInterfaceValue(Dictionary<string, object> values, string id)
        {
            if (!values.ContainsKey(id))
            {
                throw new InvalidOperationException($"Could not find value for interface {id}");
            }
            return values[id];
        }

        private void ValidateNodeCalculationOutput(Node node, Dictionary<string, object> outputs)
        {
            // Ensure all outputs are present
            foreach (var output in node.Outputs.Keys)
            {
                if (!outputs.ContainsKey(output))
                {
                    throw new InvalidOperationException(
                        $"Node {node.Id} ({node.Type}) did not provide output for '{output}'");
                }
            }
        }

        public void InvalidateCache()
        {
            _recalculateOrder = true;
        }
    }

    public class NodeCalculationEventArgs : EventArgs
    {
        public Node Node { get; set; }
        public Dictionary<string, object> Values { get; set; }
    }
}