using System;
using System.Text.Json.Serialization;
using Sandbox;

namespace MANIFOLD.AnimGraph {
    public interface INodeReferenceProvider {
        public NodeReference Reference { get; }
    }
    
    /// <summary>
    /// Serializable reference to another node.
    /// </summary>
    public record NodeReference(Guid? ID) : IValid {
        public Guid? ID { get; set; } = ID;
        
        [JsonIgnore]
        public bool IsValid => ID.HasValue;

        public static implicit operator NodeReference(BaseNode node) {
            if (node == null) return default;
            return new NodeReference(node.ID);
        }

        public override string ToString() {
            return ID.ToString();
        }
    }
    
    public abstract class BaseNode {
        public Guid ID { get; set; } = Guid.NewGuid();
        
        [JsonIgnore]
        public abstract string DisplayName { get; }
        [Hide]
        public Vector2 Position { get; set; }
    }
}
