using System;
using System.Text.Json.Serialization;
using Sandbox;

namespace MANIFOLD.AnimGraph {
    
    
    public abstract class BaseNode {
        [JsonIgnore, Hide]
        public AnimGraph Graph { get; set; }
        public Guid ID { get; set; } = Guid.NewGuid();
        
        [JsonIgnore]
        public abstract string DisplayName { get; }
        [Hide]
        public Vector2 Position { get; set; }

        public override int GetHashCode() {
            return ID.GetHashCode();
        }
    }
}
