namespace MANIFOLD.Animation {
    public interface ITrack {
        public string Name { get; set; }
        public int FrameCount { get; }
    }

    public interface ITrack<T> : ITrack {
        public T Get(int frame);
        public T GetNext(int frame);
    }
}
