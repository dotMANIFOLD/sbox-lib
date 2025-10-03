using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Sandbox;

namespace MANIFOLD.Animation {
    public class RawTrack<T> : BoneTrack<T> {
        public SortedDictionary<int, T> KeyFrames { get; set; } = new();

        [ReadOnly, JsonIgnore]
        public override int FrameCount => KeyFrames.Count;
        [Hide, JsonIgnore]
        public override bool Loaded => true;
        
        public override T Get(int frame) {
            if (frame < 0) {
                throw new ArgumentOutOfRangeException(nameof(frame), "Frame must be 0 or greater");
            }
            
            if (KeyFrames.TryGetValue(frame, out T keyFrame)) return keyFrame;
            int lastFrame = KeyFrames.Keys.Last(x => frame > x);
            return KeyFrames[lastFrame];
        }

        public override T GetNext(int frame) {
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
