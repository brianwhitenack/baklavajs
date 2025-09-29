using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaklavaDependencyEngine.Core.Models;

namespace BaklavaDependencyEngine.Core.ExampleNodes
{
    public class MeasurementAreaSumNode : Node
    {
        public MeasurementAreaSumNode() : base("MeasurementAreaSumNode", "Area Sum")
        {
        }

        protected override void InitializeInterfaces()
        {
            AddInput("measurements", new NodeInterface("Measurements", new List<Measurement>()));
            AddOutput("areaSum", new NodeInterface("Area Sum", 0.0, false));
        }

        public override async Task<Dictionary<string, object>> Calculate(
            Dictionary<string, object> inputs,
            CalculationContext context)
        {
            var measurements = inputs["measurements"] as List<Measurement> ?? new List<Measurement>();

            double areaSum = 0.0;
            foreach (var measurement in measurements.Where(m => m != null))
            {
                areaSum += measurement.Area;
            }

            return await Task.FromResult(new Dictionary<string, object>
            {
                ["areaSum"] = areaSum
            });
        }
    }
}