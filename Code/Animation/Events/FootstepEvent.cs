using System.Text.Json.Serialization;
using Sandbox;

namespace MANIFOLD.Animation {
    public record struct FootstepEvent {
        public enum FootSide : int { Left, Right }
        
        public FootSide Foot { get; set; }
        public string Attachment { get; set; }
        public float Volume { get; set; }
        
        [Hide, JsonIgnore]
        public Transform Transform { get; set; }
    }
}
