using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaklavaDependencyEngine.Core.Models;

namespace BaklavaDependencyEngine.Core.ExampleNodes
{
    public class PartCalculationOutputNode : Node
    {
        public PartCalculationOutputNode() : base("PartCalculationOutputNode", "Calculation End")
        {
        }

        protected override void InitializeInterfaces()
        {
            AddInput("parts", new NodeInterface("Parts", new List<Part>()));
        }

        public override async Task<Dictionary<string, object>> Calculate(
            Dictionary<string, object> inputs,
            CalculationContext context)
        {
            List<Part> parts = new List<Part>();

            // Handle both single Part and List<Part> inputs
            if (inputs["parts"] is List<Part> partList)
            {
                parts = partList;
            }
            else if (inputs["parts"] is Part singlePart)
            {
                parts = new List<Part> { singlePart };
            }

            // Log the final parts for output
            System.Console.WriteLine("=== Part Calculation Output ===");
            System.Console.WriteLine($"Total parts received: {parts.Count}");

            foreach (var part in parts.Where(p => p != null))
            {
                System.Console.WriteLine($"  - {part}");
            }
            System.Console.WriteLine("==============================");

            // This is typically an end node, so no outputs
            return await Task.FromResult(new Dictionary<string, object>());
        }
    }
}