using BaklavaDependencyEngine.Core.Models;

namespace BaklavaDependencyEngine.Core.ExampleNodes
{
    public class DisplayNode : Node
    {
        public DisplayNode() : base("DisplayNode", "Display")
        {
        }

        protected override void InitializeInterfaces()
        {
            AddInput("value", new NodeInterface("Value", null));
        }

        public override async Task<Dictionary<string, object>> Calculate(
            Dictionary<string, object> inputs,
            CalculationContext context)
        {
            var value = inputs["value"];
            Console.WriteLine($"Display Node: {value}");

            // Display nodes typically don't have outputs, but we need to return something
            return await Task.FromResult(new Dictionary<string, object>());
        }
    }
}