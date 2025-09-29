using BaklavaDependencyEngine.Core.Models;

namespace BaklavaDependencyEngine.Core.ExampleNodes
{
    public class StringVariableNode : Node
    {
        public StringVariableNode() : base("StringVariable", "String Variable")
        {
        }

        protected override void InitializeInterfaces()
        {
            AddOutput("result", new NodeInterface("Value", "Value", false));
        }

        public override async Task<Dictionary<string, object>> Calculate(
            Dictionary<string, object> inputs,
            CalculationContext context)
        {
            // This node just outputs its configured value
            return await Task.FromResult(new Dictionary<string, object>
            {
                ["result"] = Outputs["result"].Value
            });
        }
    }
}