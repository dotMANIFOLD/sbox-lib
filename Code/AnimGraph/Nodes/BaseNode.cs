using System;
using System.Text.Json.Serialization;

namespace MANIFOLD.AnimGraph {
    public abstract class BaseNode {
        public Guid ID { get; set; } = Guid.NewGuid();
        
        [JsonIgnore]
        public abstract string DisplayName { get; }
        public Vector2 Position { get; set; }
    }
}
