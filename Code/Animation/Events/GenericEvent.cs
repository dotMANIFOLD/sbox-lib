namespace MANIFOLD.Animation {
    public record GenericEvent : IEvent {
        public string Type { get; set; }
        public int Int { get; set; }
        public float Float { get; set; }
        public Vector3 Vector { get; set; }
        public string String { get; set; }
    }
}
