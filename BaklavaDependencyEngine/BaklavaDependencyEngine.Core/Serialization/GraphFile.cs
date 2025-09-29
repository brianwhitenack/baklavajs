
using BaklavaDependencyEngine.Core.Models;

namespace BaklavaDependencyEngine.Core.Serialization
{
    public class GraphFile
    {
        public Graph Graph { get; set; }

        public GraphFile(Graph graph)
        {
            Graph = graph;
        }

        public void Restore()
        {
            Graph.Restore();
        }
    }
}