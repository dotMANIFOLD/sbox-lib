using System;
using System.Text.Json.Serialization;
using Sandbox;

namespace MANIFOLD.Animation {
    public abstract class BoneTrack : ITrack {
        public string Name { get; set; } = "Track";
        public string TargetBone { get; set; }
        
        [ReadOnly, JsonIgnore]
        public abstract int FrameCount { get; }
        [ReadOnly, JsonIgnore]
        public abstract bool Loaded { get; }
        [Hide, JsonIgnore]
        public abstract Type DataType { get; }
        
        string ITrack.Name => $"{Name} ({TargetBone})";
        
        public virtual void Load() {
            
        }

        public virtual void Unload() {
            
        }

        public virtual void CompressData() {
            
        }
    }

    public abstract class BoneTrack<T> : BoneTrack, ITrack<T> {
        [Hide, JsonIgnore]
        public sealed override Type DataType => typeof(T);

        public abstract T Get(int frame);
        public abstract T GetNext(int frame);
    }
}
