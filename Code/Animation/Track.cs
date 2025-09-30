using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace MANIFOLD.Animation {
    public abstract class Track {
        public string Name { get; set; } = "Track";
        public string TargetBone { get; set; }
        
        [JsonIgnore]
        public abstract int FrameCount { get; }
        [JsonIgnore]
        public abstract bool Ready { get; }

        public virtual void Compress() {
            
        }

        public virtual void Decompress() {
            
        }

        public virtual void Reset() {
            
        }
        
        public override string ToString() {
            string str = string.IsNullOrEmpty(Name) ? "No Name" : Name;
            if (!string.IsNullOrEmpty(TargetBone)) {
                str = TargetBone + " : " + str;
            }
            return str;
        }
    }
    
    public abstract class Track<T> : Track {
        public abstract T Get(int frame);

        public abstract T GetNext(int frame);
    }
}
