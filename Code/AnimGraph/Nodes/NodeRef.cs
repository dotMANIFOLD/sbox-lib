using System;
using System.Text.Json.Serialization;
using Sandbox;

namespace MANIFOLD.AnimGraph {
    /// <summary>
    /// Serializable reference to another node.
    /// </summary>
    public record struct NodeRef(Guid? ID = null) : IValid {
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
    
    /// <summary>
    /// Provider for references. Used when nodes need extra data per reference.
    /// </summary>
    /// <remarks>Your reference must be named <c>Reference</c></remarks>
    public interface INodeRefProvider {
        public NodeRef Reference { get; }
    }
}
