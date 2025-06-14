using System;
using System.Text.Json.Serialization;
using Sandbox;

namespace MANIFOLD.AnimGraph {
    /// <summary>
    /// Serializable reference to another node.
    /// </summary>
    public record NodeRef(Guid? ID) : IValid {
        public Guid? ID { get; set; } = ID;
        
        [JsonIgnore]
        public bool IsValid => ID.HasValue;

        public static implicit operator NodeRef(BaseNode node) {
            if (node == null) return default;
            return new NodeRef(node.ID);
        }

        public override string ToString() {
            return ID.ToString();
        }
    }
    
    public interface INodeRefProvider {
        public NodeRef Reference { get; }
    }
}
