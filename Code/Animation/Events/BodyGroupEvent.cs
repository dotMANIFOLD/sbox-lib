namespace MANIFOLD.Animation {
    public record BodyGroupEvent : IEvent {
        public string BodyGroup { get; set; }
        public int Value { get; set; }
    }
}
