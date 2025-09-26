using System;
using System.Collections.Generic;
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

        [JsonIgnore]
        public Node ParentNode { get; set; }

        public NodeInterface(string name, object defaultValue = null, bool isInput = true)
        {
            Name = name;
            Value = defaultValue;
            IsInput = isInput;
        }
    }
}