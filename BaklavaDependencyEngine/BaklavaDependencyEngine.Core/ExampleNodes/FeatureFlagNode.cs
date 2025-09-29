using System.Collections.Generic;
using System.Threading.Tasks;
using BaklavaDependencyEngine.Core.Models;

namespace BaklavaDependencyEngine.Core.ExampleNodes
{
    public class FeatureFlagNode : Node
    {
        private readonly Dictionary<string, bool> _featureFlags;

        public FeatureFlagNode() : base("FeatureFlagNode", "Feature Flag")
        {
            // Initialize some default feature flags
            _featureFlags = new Dictionary<string, bool>
            {
                ["EnableNewFeature"] = true,
                ["EnableDebugMode"] = false,
                ["EnableBetaFeatures"] = true,
                ["Feature Flag Name"] = false
            };
        }

        protected override void InitializeInterfaces()
        {
            AddInput("featureFlagName", new NodeInterface("Feature Flag Name", "Feature Flag Name"));
            AddOutput("featureFlagValue", new NodeInterface("Feature Flag Value", false, false));
        }

        public override async Task<Dictionary<string, object>> Calculate(
            Dictionary<string, object> inputs,
            CalculationContext context)
        {
            var flagName = inputs["featureFlagName"]?.ToString() ?? "Feature Flag Name";
            var flagValue = _featureFlags.ContainsKey(flagName) ? _featureFlags[flagName] : false;

            return await Task.FromResult(new Dictionary<string, object>
            {
                ["featureFlagValue"] = flagValue
            });
        }

        public void SetFeatureFlag(string name, bool value)
        {
            _featureFlags[name] = value;
        }
    }
}