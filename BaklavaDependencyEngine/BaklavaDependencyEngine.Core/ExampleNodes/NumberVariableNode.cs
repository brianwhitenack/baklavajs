using System.Collections.Generic;
using System.Threading.Tasks;
using BaklavaDependencyEngine.Core.Models;

namespace BaklavaDependencyEngine.Core.ExampleNodes
{
    public class NumberVariableNode : Node
    {
        public NumberVariableNode() : base("NumberVariableNode", "Number Variable")
        {
        }

        protected override void InitializeInterfaces()
        {
            AddOutput("result", new NodeInterface("Value", 0.0, false));
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