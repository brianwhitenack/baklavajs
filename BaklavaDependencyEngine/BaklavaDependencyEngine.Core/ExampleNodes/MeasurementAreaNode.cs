using BaklavaDependencyEngine.Core.Models;

namespace BaklavaDependencyEngine.Core.ExampleNodes
{
    public class MeasurementAreaNode : Node
    {
        public MeasurementAreaNode() : base("MeasurementAreaNode", "Measurement Area")
        {
        }

        protected override void InitializeInterfaces()
        {
            AddInput("measurement", new NodeInterface("Measurement", null));
            AddOutput("area", new NodeInterface("Area", 0.0, false));
        }

        public override async Task<Dictionary<string, object>> Calculate(
            Dictionary<string, object> inputs,
            CalculationContext context)
        {
            var measurement = inputs["measurement"] as Measurement;
            var area = measurement?.Area ?? 0.0;

            return await Task.FromResult(new Dictionary<string, object>
            {
                ["area"] = area
            });
        }
    }
}