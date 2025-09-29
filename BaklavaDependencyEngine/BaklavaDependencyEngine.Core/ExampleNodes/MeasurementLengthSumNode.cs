using BaklavaDependencyEngine.Core.Models;

namespace BaklavaDependencyEngine.Core.ExampleNodes
{
    public class MeasurementLengthSumNode : Node
    {
        public MeasurementLengthSumNode() : base("MeasurementLengthSumNode", "Length Sum")
        {
        }

        protected override void InitializeInterfaces()
        {
            AddInput("measurements", new NodeInterface("Measurements", new List<Measurement>()));
            AddOutput("lengthSum", new NodeInterface("Length Sum", 0.0, false));
        }

        public override async Task<Dictionary<string, object>> Calculate(
            Dictionary<string, object> inputs,
            CalculationContext context)
        {
            var measurements = inputs["measurements"] as List<Measurement> ?? new List<Measurement>();

            double lengthSum = 0.0;
            foreach (var measurement in measurements.Where(m => m != null))
            {
                lengthSum += measurement.Length;
            }

            return await Task.FromResult(new Dictionary<string, object>
            {
                ["lengthSum"] = lengthSum
            });
        }
    }
}