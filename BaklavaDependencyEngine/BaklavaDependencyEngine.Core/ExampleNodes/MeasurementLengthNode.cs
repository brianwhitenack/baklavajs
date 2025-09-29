using System.Collections.Generic;
using System.Threading.Tasks;
using BaklavaDependencyEngine.Core.Models;

namespace BaklavaDependencyEngine.Core.ExampleNodes
{
    public class MeasurementLengthNode : Node
    {
        public MeasurementLengthNode() : base("MeasurementLengthNode", "Measurement Length")
        {
        }

        protected override void InitializeInterfaces()
        {
            AddInput("measurement", new NodeInterface("Measurement", null));
            AddOutput("length", new NodeInterface("Length", 0.0, false));
        }

        public override async Task<Dictionary<string, object>> Calculate(
            Dictionary<string, object> inputs,
            CalculationContext context)
        {
            var measurement = inputs["measurement"] as Measurement;
            var length = measurement?.Length ?? 0.0;

            return await Task.FromResult(new Dictionary<string, object>
            {
                ["length"] = length
            });
        }
    }
}