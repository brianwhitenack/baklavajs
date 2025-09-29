using Newtonsoft.Json;

namespace BaklavaDependencyEngine.Core.Models
{
    public class NodeInterface
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public object Value { get; set; }
        public bool AllowMultipleConnections { get; set; }
        public int ConnectionCount { get; set; }
        public bool IsInput { get; set; }
        public bool Port { get; set; } = true;
        public string ParentNodeId { get; private set; }

        [JsonIgnore]
        public Node ParentNode { get; private set; }

        public NodeInterface(string name, object defaultValue = null, bool isInput = true)
        {
            Name = name;
            Value = defaultValue;
            IsInput = isInput;
        }

        public void Restore(Graph _, Node parentNode)
        {
            SetParentNode(parentNode);
        }

        public void SetParentNode(Node parentNode)
        {
            ParentNode = parentNode ?? throw new ArgumentNullException(nameof(parentNode));
        }
    }
}