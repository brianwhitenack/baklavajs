using System;
using System.Collections.Generic;
using System.Linq;

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
                c.From.ParentNode == node || c.To.ParentNode == node);

            Nodes.Remove(node);
            node.ParentGraph = null;
        }

        public void AddConnection(Connection connection)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            // Update connection counts
            connection.To.ConnectionCount++;

            Connections.Add(connection);
        }

        public void RemoveConnection(Connection connection)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            // Update connection counts
            connection.To.ConnectionCount--;

            Connections.Remove(connection);
        }

        public List<Connection> GetConnectionsFromNode(Node node)
        {
            return Connections.Where(c => c.From.ParentNode == node).ToList();
        }

        public List<Connection> GetConnectionsToNode(Node node)
        {
            return Connections.Where(c => c.To.ParentNode == node).ToList();
        }
    }
}