using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Sandbox;

namespace MANIFOLD.Animation {
    public abstract class EventTrack : ITrack, ITrack<IEvent> {
        public string Name { get; set; } = "No Name";
        
        [ReadOnly, JsonIgnore]
        public abstract int FrameCount { get; }
        [Hide, JsonIgnore]
        public abstract Type DataType { get; }

        internal abstract IEvent GetInternal(int frame);
        internal abstract IEvent GetNextInternal(int frame);
        
        IEvent ITrack<IEvent>.Get(int frame) => GetInternal(frame);
        IEvent ITrack<IEvent>.GetNext(int frame) => GetNextInternal(frame);
    }

    public class EventTrack<T> : EventTrack, ITrack<T> where T : class, IEvent {
        public SortedDictionary<int, T> Events { get; set; } = new();

        [ReadOnly, JsonIgnore]
        public override int FrameCount => Events.Count;
        [Hide, JsonIgnore]
        public sealed override Type DataType => typeof(T);

        internal override IEvent GetInternal(int frame) {
            return Get(frame);
        }

        internal override IEvent GetNextInternal(int frame) {
            return GetNext(frame);
        }

        public T Get(int frame) {
            if (frame < 0) {
                throw new ArgumentOutOfRangeException(nameof(frame), "Frame must be 0 or greater");
            }
            
            if (Events.TryGetValue(frame, out T keyFrame)) return keyFrame;
            return null;
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
