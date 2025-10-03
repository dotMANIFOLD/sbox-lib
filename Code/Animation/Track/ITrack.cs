using System;

namespace MANIFOLD.Animation {
    public interface ITrack {
        public string Name { get; }
        public int FrameCount { get; }
        public Type DataType { get; }
    }

    public interface ITrack<T> : ITrack {
        public T Get(int frame);
        public T GetNext(int frame);
    }
}
