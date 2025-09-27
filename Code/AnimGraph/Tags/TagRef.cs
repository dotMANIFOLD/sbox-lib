using System;
using System.Text.Json.Serialization;
using Sandbox;

namespace MANIFOLD.AnimGraph {
    /// <summary>
    /// Serializable reference to a tag.
    /// </summary>
    /// <param name="ID">ID of the tag.</param>
    public record struct TagRef(Guid? ID) : IValid {
        [JsonIgnore]
        public bool IsValid => ID.HasValue;

        public static implicit operator TagRef(Tag tag) {
            if (tag == null) return default;
            return new TagRef(tag.ID);
        }

        public override string ToString() {
            return ID.ToString();
        }
    }
}
