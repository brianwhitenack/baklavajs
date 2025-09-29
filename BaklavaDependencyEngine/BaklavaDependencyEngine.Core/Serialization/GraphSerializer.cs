using BaklavaDependencyEngine.Core.Models;

using Newtonsoft.Json;

namespace BaklavaDependencyEngine.Core.Serialization
{
    /// <summary>
    /// GraphSerializer V3 using JsonSubTypes for automatic polymorphic serialization
    /// No manual registration needed - all node types are handled via attributes on the Node base class
    /// </summary>
    public class GraphSerializer
    {
        private readonly JsonSerializerSettings _settings;

        public GraphSerializer()
        {
            _settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.None,
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Include
            };
        }

        public string SerializeGraph(Graph graph)
        {
            GraphFile graphFile = new GraphFile(graph);

            return JsonConvert.SerializeObject(graphFile, _settings);
        }

        public Graph DeserializeGraph(string json)
        {
            GraphFile graphFile = JsonConvert.DeserializeObject<GraphFile>(json, _settings);

            graphFile.Restore();

            return graphFile.Graph;
        }
    }
}