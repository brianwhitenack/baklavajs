using BaklavaDependencyEngine.Core.Models;

namespace BaklavaDependencyEngine.Core.ExampleNodes
{
    public class FilterToTypeNode : Node
    {
        public FilterToTypeNode() : base("FilterToTypeNode", "Filter To Type")
        {
        }

        protected override void InitializeInterfaces()
        {
            AddInput("inputMeasurements", new NodeInterface("Measurements", new List<Measurement>()));
            AddInput("type", new NodeInterface("Type", ""));
            AddOutput("outputMeasurements", new NodeInterface("Filtered Measurements", new List<Measurement>(), false));
        }

        public override async Task<Dictionary<string, object>> Calculate(
            Dictionary<string, object> inputs,
            CalculationContext context)
        {
            var outputMeasurements = new List<Measurement>();
            var typeFilter = inputs["type"]?.ToString() ?? "";

            if (inputs["inputMeasurements"] is List<Measurement> measurements)
            {
                foreach (var measurement in measurements)
                {
                    if (measurement != null && measurement.Type == typeFilter)
                    {
                        outputMeasurements.Add(measurement);
                    }
                }
            }

            return await Task.FromResult(new Dictionary<string, object>
            {
                ["outputMeasurements"] = outputMeasurements
            });
        }
    }
}