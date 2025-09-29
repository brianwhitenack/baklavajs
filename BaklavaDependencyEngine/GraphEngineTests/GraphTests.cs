using BaklavaDependencyEngine.Core;
using BaklavaDependencyEngine.Core.Models;
using BaklavaDependencyEngine.Core.Serialization;

namespace GraphEngineTests
{
    public class GraphTests
    {
        [Fact]
        public async Task Test1()
        {
            GraphSerializer serializer = new GraphSerializer();
            DependencyEngine engine = new DependencyEngine();

            string json = File.ReadAllText("C:\\Users\\Brian.Whitenack\\source\\repos\\Baklava\\packages\\renderer-vue\\playground\\SampleGraphs\\Brick.json");
            Graph graph = serializer.DeserializeGraph(json);

            CalculationResult result = await engine.RunGraph(graph, new CalculationContext());
        }
    }
}