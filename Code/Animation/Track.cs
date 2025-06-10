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
        
        public override string ToString() {
            string str = string.IsNullOrEmpty(Name) ? "No Name" : Name;
            if (!string.IsNullOrEmpty(TargetBone)) {
                str = TargetBone + " : " + str;
            }
            return str;
        }
    }
    
    public abstract class Track<T> : Track {
        public Dictionary<int, T> KeyFrames { get; set; } = new();

        [JsonIgnore]
        public override int FrameCount => KeyFrames.Count == 0 ? 0 : KeyFrames.Keys.Max();

        public T Get(int frame) {
            if (frame < 0) {
                throw new ArgumentOutOfRangeException(nameof(frame), "Frame must be 0 or greater");
            }
            
            if (KeyFrames.TryGetValue(frame, out T keyFrame)) return keyFrame;
            int lastFrame = KeyFrames.Keys.Last(x => frame > x);
            return KeyFrames[lastFrame];
        }

        public T GetNext(int frame) {
            if (frame < 0) {
                throw new ArgumentOutOfRangeException(nameof(frame), "Frame must be 0 or greater");
            }
            
            int nextFrame = KeyFrames.Keys.FirstOrDefault(x => frame < x, -1);
            if (nextFrame == -1) {
                return Get(frame);
            }
            return KeyFrames[nextFrame];
        }
    }
}
