using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Sandbox;

namespace MANIFOLD.Animation {
    public abstract class EventTrack : ITrack {
        public string Name { get; set; } = "No Name";
        
        [ReadOnly, JsonIgnore]
        public abstract int FrameCount { get; }
        [Hide, JsonIgnore]
        public abstract Type DataType { get; }
    }

    public class EventTrack<T> : EventTrack, ITrack<T> {
        public SortedDictionary<int, T> Events { get; set; } = new();

        [ReadOnly, JsonIgnore]
        public override int FrameCount => Events.Count;
        [Hide, JsonIgnore]
        public sealed override Type DataType => typeof(T);

        public T Get(int frame) {
            if (frame < 0) {
                throw new ArgumentOutOfRangeException(nameof(frame), "Frame must be 0 or greater");
            }
            
            if (Events.TryGetValue(frame, out T keyFrame)) return keyFrame;
            int lastFrame = Events.Keys.Last(x => frame > x);
            return Events[lastFrame];
        }

        public T GetNext(int frame) {
            if (frame < 0) {
                throw new ArgumentOutOfRangeException(nameof(frame), "Frame must be 0 or greater");
            }
            
            int nextFrame = Events.Keys.FirstOrDefault(x => frame < x, -1);
            if (nextFrame == -1) {
                return Get(frame);
            }
            return Events[nextFrame];
        }
    }
}
