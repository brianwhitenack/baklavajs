using BaklavaDependencyEngine.Core.ExampleNodes;

using JsonSubTypes;

using Newtonsoft.Json;

namespace BaklavaDependencyEngine.Core.Models
{
    [JsonConverter(typeof(JsonSubtypes), "Type")]
    [JsonSubtypes.KnownSubType(typeof(NumberNode), "NumberNode")]
    [JsonSubtypes.KnownSubType(typeof(MathNode), "MathNode")]
    [JsonSubtypes.KnownSubType(typeof(DisplayNode), "DisplayNode")]
    [JsonSubtypes.KnownSubType(typeof(StringVariableNode), "StringVariable")]
    [JsonSubtypes.KnownSubType(typeof(NumberVariableNode), "NumberVariable")]
    [JsonSubtypes.KnownSubType(typeof(CreatePartNode), "CreatePartNode")]
    [JsonSubtypes.KnownSubType(typeof(PartCalculationOutputNode), "PartCalculationOutputNode")]
    [JsonSubtypes.KnownSubType(typeof(MeasurementAreaNode), "MeasurementAreaNode")]
    [JsonSubtypes.KnownSubType(typeof(MeasurementLengthNode), "MeasurementLengthNode")]
    [JsonSubtypes.KnownSubType(typeof(MeasurementAreaSumNode), "MeasurementAreaSumNode")]
    [JsonSubtypes.KnownSubType(typeof(MeasurementLengthSumNode), "MeasurementLengthSumNode")]
    [JsonSubtypes.KnownSubType(typeof(BuildingMeasurementsNode), "BuildingMeasurementsNode")]
    [JsonSubtypes.KnownSubType(typeof(FeatureFlagNode), "FeatureFlagNode")]
    [JsonSubtypes.KnownSubType(typeof(FilterToTypeNode), "FilterToTypeNode")]
    [JsonSubtypes.KnownSubType(typeof(FilterToSelectionNode), "FilterToSelectionNode")]
    public abstract class Node
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Type { get; set; }
        public string Title { get; set; }
        public Dictionary<string, NodeInterface> Inputs { get; set; } = new Dictionary<string, NodeInterface>();
        public Dictionary<string, NodeInterface> Outputs { get; set; } = new Dictionary<string, NodeInterface>();

        [JsonIgnore]
        public Graph ParentGraph { get; set; }

        protected Node(string type, string title)
        {
            Type = type;
            Title = title;
            InitializeInterfaces();
        }

        protected virtual void InitializeInterfaces()
        {
            // Override in derived classes to set up inputs and outputs
        }

        public virtual async Task<Dictionary<string, object>> Calculate(
            Dictionary<string, object> inputs,
            CalculationContext context)
        {
            // Default implementation for nodes without calculation logic
            var result = new Dictionary<string, object>();
            foreach (var output in Outputs)
            {
                result[output.Key] = output.Value.Value;
            }
            return await Task.FromResult(result);
        }

        protected void AddInput(string key, NodeInterface nodeInterface)
        {
            nodeInterface.IsInput = true;
            nodeInterface.SetParentNode(this);
            Inputs[key] = nodeInterface;
        }

        protected void AddOutput(string key, NodeInterface nodeInterface)
        {
            nodeInterface.IsInput = false;
            nodeInterface.SetParentNode(this);
            Outputs[key] = nodeInterface;
        }

        public void Restore(Graph graph)
        {
            ParentGraph = graph;
            foreach (NodeInterface input in Inputs.Values)
            {
                input.Restore(graph, this);
            }
            foreach (NodeInterface output in Outputs.Values)
            {
                output.Restore(graph, this);
            }
        }
    }

    public class CalculationContext
    {
        public object GlobalValues { get; set; }
        public DependencyEngine Engine { get; set; }
    }
}