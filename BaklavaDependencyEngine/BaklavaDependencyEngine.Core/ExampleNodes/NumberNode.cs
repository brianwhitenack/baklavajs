using BaklavaDependencyEngine.Core.Models;

namespace BaklavaDependencyEngine.Core.ExampleNodes
{
    public class NumberNode : Node
    {
        public NumberNode() : base("NumberNode", "Number")
        {
        }

        protected override void InitializeInterfaces()
        {
            AddOutput("value", new NodeInterface("Value", 42.0, false));
        }

        public override async Task<Dictionary<string, object>> Calculate(
            Dictionary<string, object> inputs,
            CalculationContext context)
        {
            // This node just outputs its configured value
            return await Task.FromResult(new Dictionary<string, object>
            {
                ["value"] = Outputs["value"].Value
            });
        }
    }
}