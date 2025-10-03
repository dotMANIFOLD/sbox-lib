using System.Text.Json.Serialization;

namespace MANIFOLD.Animation {
    public abstract class BoneTrack : ITrack {
        public string Name { get; set; } = "Track";
        public string TargetBone { get; set; }
        
        [JsonIgnore]
        public abstract int FrameCount { get; }
        [JsonIgnore]
        public abstract bool Loaded { get; }

        public virtual void Load() {
            
        }

        public virtual void Unload() {
            
        }

        public virtual void CompressData() {
            
        }
        
        public override string ToString() {
            string str = string.IsNullOrEmpty(Name) ? "No Name" : Name;
            if (!string.IsNullOrEmpty(TargetBone)) {
                str = TargetBone + " : " + str;
            }
            return str;
        }
    }

    public abstract class BoneTrack<T> : BoneTrack, ITrack<T> {
        public abstract T Get(int frame);
        public abstract T GetNext(int frame);
    }
}
