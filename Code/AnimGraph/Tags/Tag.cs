using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Sandbox;

namespace MANIFOLD.AnimGraph {
    public class Tag {
        private class ActiveHandle : IDisposable {
            public readonly Tag owner;

            public ActiveHandle(Tag owner) {
                this.owner = owner;
                owner.AddHandle(this);
            }

            public void Dispose() {
                owner.RemoveHandle(this);
            }
        }
        
        public enum TagType { Event, Internal }

        private HashSet<ActiveHandle> activeHandles = new HashSet<ActiveHandle>();
        
        public Guid ID { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "Unnamed Tag";
        public TagType Type { get; set; }

        [Hide, JsonIgnore]
        public int HandleCount => activeHandles.Count;
        [Hide, JsonIgnore]
        public bool State => activeHandles.Count > 0;
        [Hide, JsonIgnore]
        public Action<Tag> OnStateChanged { get; set; }

        public IDisposable CreateHandle() {
            return new ActiveHandle(this);
        }
        
        public Tag Clone() {
            return new Tag() {
                ID = ID,
                Name = Name,
                Type = Type,
            };
        }

        private void AddHandle(ActiveHandle handle) {
            activeHandles.Add(handle);
            if (activeHandles.Count == 1) {
                OnStateChanged?.Invoke(this);
                Log.Info($"Tag {Name} now active");
            }
        }

        private void RemoveHandle(ActiveHandle handle) {
            activeHandles.Remove(handle);
            if (activeHandles.Count == 0) {
                OnStateChanged?.Invoke(this);
                Log.Info($"Tag {Name} no longer active");
            }
        }
    }
}
