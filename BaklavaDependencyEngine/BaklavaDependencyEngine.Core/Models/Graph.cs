namespace BaklavaDependencyEngine.Core.Models
{
    public class Graph
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public List<Node> Nodes { get; set; } = new List<Node>();
        public List<Connection> Connections { get; set; } = new List<Connection>();

        public void AddNode(Node node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            node.ParentGraph = this;
            Nodes.Add(node);
        }

        public void RemoveNode(Node node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            // Remove all connections involving this node
            Connections.RemoveAll(c =>
                c.FromInterface.ParentNode == node || c.ToInterface.ParentNode == node);

            Nodes.Remove(node);
            node.ParentGraph = null;
        }

        public void AddConnection(Connection connection)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            // Update connection counts
            connection.ToInterface.ConnectionCount++;

            Connections.Add(connection);
        }

        public void RemoveConnection(Connection connection)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            // Update connection counts
            connection.ToInterface.ConnectionCount--;

            Connections.Remove(connection);
        }

        public List<Connection> GetConnectionsFromNode(Node node)
        {
            return Connections.Where(c => c.FromInterface.ParentNode == node).ToList();
        }

        public List<Connection> GetConnectionsToNode(Node node)
        {
            return Connections.Where(c => c.ToInterface.ParentNode == node).ToList();
        }

        public NodeInterface GetNodeInterfaceById(string id)
        {
            foreach (Node node in Nodes)
            {
                KeyValuePair<string, NodeInterface> nodeInterface = node.Inputs.Concat(node.Outputs).FirstOrDefault(ni => ni.Value.Id == id);
                if (nodeInterface.Value != null)
                {
                    return nodeInterface.Value;
                }
            }

            throw new InvalidOperationException("could not find node interface with id " + id);
        }

        public Node GetNodeById(string id)
        {
            Node node = Nodes.FirstOrDefault(n => n.Id == id);
            if (node != null)
            {
                return node;
            }
            else
            {
                throw new InvalidOperationException("could not find node with id " + id);
            }
        }

        public void Restore()
        {
            foreach (Node node in Nodes)
            {
                node.Restore(this);
            }

            foreach (Connection connection in Connections)
            {
                connection.Restore(this);
            }
        }
    }
}