using System;

namespace BaklavaDependencyEngine.Core.Models
{
    public class Connection
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public NodeInterface From { get; set; }
        public NodeInterface To { get; set; }

        public Connection(NodeInterface from, NodeInterface to)
        {
            From = from ?? throw new ArgumentNullException(nameof(from));
            To = to ?? throw new ArgumentNullException(nameof(to));
        }
    }
}