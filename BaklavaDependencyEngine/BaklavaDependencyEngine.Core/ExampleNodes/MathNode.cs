using BaklavaDependencyEngine.Core.Models;

namespace BaklavaDependencyEngine.Core.ExampleNodes
{
    public class MathNode : Node
    {
        public enum Operation
        {
            Add,
            Subtract,
            Multiply,
            Divide
        }

        public Operation MathOperation { get; set; } = Operation.Add;

        public MathNode() : base("MathNode", "Math")
        {
        }

        protected override void InitializeInterfaces()
        {
            AddInput("number1", new NodeInterface("Number 1", 0.0));
            AddInput("number2", new NodeInterface("Number 2", 0.0));
            AddOutput("result", new NodeInterface("Result", 0.0, false));
        }

        public override async Task<Dictionary<string, object>> Calculate(
            Dictionary<string, object> inputs,
            CalculationContext context)
        {
            var num1 = Convert.ToDouble(inputs["number1"]);
            var num2 = Convert.ToDouble(inputs["number2"]);
            double result;

            switch (MathOperation)
            {
                case Operation.Add:
                    result = num1 + num2;
                    break;
                case Operation.Subtract:
                    result = num1 - num2;
                    break;
                case Operation.Multiply:
                    result = num1 * num2;
                    break;
                case Operation.Divide:
                    if (Math.Abs(num2) < 0.0001)
                        throw new DivideByZeroException();
                    result = num1 / num2;
                    break;
                default:
                    throw new InvalidOperationException($"Unknown operation: {MathOperation}");
            }

            return await Task.FromResult(new Dictionary<string, object>
            {
                ["result"] = result
            });
        }
    }
}