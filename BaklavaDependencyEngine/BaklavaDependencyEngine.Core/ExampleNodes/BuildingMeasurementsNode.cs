using BaklavaDependencyEngine.Core.Models;

namespace BaklavaDependencyEngine.Core.ExampleNodes
{
    public class BuildingMeasurementsNode : Node
    {
        public BuildingMeasurementsNode() : base("BuildingMeasurementsNode", "Building Measurements")
        {
        }

        protected override void InitializeInterfaces()
        {
            AddOutput("measurements", new NodeInterface("Measurements", new List<Measurement>(), false));
        }

        public override async Task<Dictionary<string, object>> Calculate(
            Dictionary<string, object> inputs,
            CalculationContext context)
        {
            // This node provides sample building measurements
            var measurements = new List<Measurement>
            {
                new Measurement("Wall", 10.0, 25.0),
                new Measurement("Floor", 12.0, 144.0),
                new Measurement("Ceiling", 12.0, 144.0),
                new Measurement("Window", 3.0, 6.0),
                new Measurement("Door", 2.5, 5.0)
            };

            return await Task.FromResult(new Dictionary<string, object>
            {
                ["measurements"] = measurements
            });
        }
    }
}