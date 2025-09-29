using Newtonsoft.Json;

namespace BaklavaDependencyEngine.Core.Models
{
    public class Connection
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string From { get; set; }
        public string To { get; set; }

        [JsonIgnore]
        public NodeInterface FromInterface { get; private set; }
        [JsonIgnore]
        public NodeInterface ToInterface { get; private set; }

        [JsonConstructor]
        private Connection() { }

        public Connection(NodeInterface from, NodeInterface to)
        {
            FromInterface = from ?? throw new ArgumentNullException(nameof(from));
            ToInterface = to ?? throw new ArgumentNullException(nameof(to));
        }

        public void SetInterfaces(NodeInterface from, NodeInterface to)
        {
            FromInterface = from ?? throw new ArgumentNullException(nameof(from));
            ToInterface = to ?? throw new ArgumentNullException(nameof(to));
            From = from.Id;
            To = to.Id;
        }

        public void Restore(Graph graph)
        {
            FromInterface = graph.GetNodeInterfaceById(From);
            ToInterface = graph.GetNodeInterfaceById(To);
        }
    }
}