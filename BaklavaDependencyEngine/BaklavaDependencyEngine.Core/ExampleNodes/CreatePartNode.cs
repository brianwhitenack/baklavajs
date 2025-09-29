using BaklavaDependencyEngine.Core.Models;

namespace BaklavaDependencyEngine.Core.ExampleNodes
{
    public class CreatePartNode : Node
    {
        public CreatePartNode() : base("CreatePartNode", "Create Part")
        {
        }

        protected override void InitializeInterfaces()
        {
            AddInput("sku", new NodeInterface("SKU", ""));
            AddInput("description", new NodeInterface("Description", ""));
            AddInput("package", new NodeInterface("Package", ""));
            AddInput("quantity", new NodeInterface("Quantity", 0));
            AddOutput("part", new NodeInterface("Part", null, false));
        }

        public override async Task<Dictionary<string, object>> Calculate(
            Dictionary<string, object> inputs,
            CalculationContext context)
        {
            var sku = inputs["sku"]?.ToString() ?? "";
            var description = inputs["description"]?.ToString() ?? "";
            var package = inputs["package"]?.ToString() ?? "";
            var quantity = Convert.ToInt32(inputs["quantity"]);

            var part = new Part(sku, description, package, quantity);

            return await Task.FromResult(new Dictionary<string, object>
            {
                ["part"] = part
            });
        }
    }
}