using System.Reflection;

using BaklavaDependencyEngine.Core.Models;

namespace BaklavaDependencyEngine.Core.ExampleNodes
{
    public class FilterToSelectionNode : Node
    {
        public FilterToSelectionNode() : base("FilterToSelectionNode", "Filter To Selection")
        {
        }

        protected override void InitializeInterfaces()
        {
            AddInput("inputMeasurements", new NodeInterface("Measurements", new List<Measurement>()));
            AddInput("selectionName", new NodeInterface("Selection Name", ""));
            AddInput("selectionValue", new NodeInterface("Selection Value", null));
            AddOutput("outputMeasurements", new NodeInterface("Filtered Measurements", new List<Measurement>(), false));
        }

        public override async Task<Dictionary<string, object>> Calculate(
            Dictionary<string, object> inputs,
            CalculationContext context)
        {
            var outputMeasurements = new List<Measurement>();
            var selectionName = inputs["selectionName"]?.ToString() ?? "";
            var selectionValue = inputs["selectionValue"];

            if (inputs["inputMeasurements"] is List<Measurement> measurements && !string.IsNullOrEmpty(selectionName))
            {
                foreach (var measurement in measurements)
                {
                    if (measurement != null)
                    {
                        // Use reflection to get the property value
                        var property = measurement.GetType().GetProperty(selectionName,
                            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                        if (property != null)
                        {
                            var measurementValue = property.GetValue(measurement);

                            // Compare values
                            if (AreValuesEqual(measurementValue, selectionValue))
                            {
                                outputMeasurements.Add(measurement);
                            }
                        }
                        // If property doesn't exist on measurement, check Selections dictionary
                        else if (measurement.Selections.ContainsKey(selectionName))
                        {
                            var measurementValue = measurement.Selections[selectionName];
                            if (AreValuesEqual(measurementValue, selectionValue))
                            {
                                outputMeasurements.Add(measurement);
                            }
                        }
                    }
                }
            }

            return await Task.FromResult(new Dictionary<string, object>
            {
                ["outputMeasurements"] = outputMeasurements
            });
        }

        private bool AreValuesEqual(object value1, object value2)
        {
            if (value1 == null && value2 == null) return true;
            if (value1 == null || value2 == null) return false;

            // Try to convert types if they're different
            if (value1.GetType() != value2.GetType())
            {
                try
                {
                    var converted = Convert.ChangeType(value2, value1.GetType());
                    return value1.Equals(converted);
                }
                catch
                {
                    return value1.ToString() == value2.ToString();
                }
            }

            return value1.Equals(value2);
        }
    }
}