using System;
using System.Text.Json.Serialization;
using Sandbox;

namespace MANIFOLD.AnimGraph {
    public class Tag {
        public enum TagType { Event, Internal }

        private bool state;
        
        public Guid ID { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "Unnamed Tag";
        [ReadOnly]
        public TagType Type { get; set; }

        [Hide, JsonIgnore]
        public bool State {
            get => state;
            set {
                var diff = state != value;
                state = value;
                if (diff) OnStateChanged?.Invoke(this);
            }
        }
        [Hide, JsonIgnore]
        public Action<Tag> OnStateChanged { get; set; }
        
        public Tag Clone() {
            return new Tag() {
                ID = ID,
                Name = Name,
                Type = Type,
                state = state
            };
        }
    }
}
