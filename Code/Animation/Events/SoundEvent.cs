namespace MANIFOLD.Animation {
    public record struct SoundEvent {
        public Sandbox.SoundEvent Event { get; set; }
        public string Attachment { get; set; }
        public Vector3 Position { get; set; }
    }
}
