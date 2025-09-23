using MANIFOLD.Animation;
using Sandbox;

namespace MANIFOLD.AnimGraph {
    public abstract record ResourceRef {
        public enum RefMode { Named, Direct }
        
        public RefMode Mode { get; set; }
        public string NamedReference { get; set; }
        
        public virtual string Name => NamedReference;
    }
    
    public record ResourceRef<T> : ResourceRef where T : GameResource, INamedResource {
        public T DirectReference { get; set; }

        public override string Name => Mode switch {
            RefMode.Direct => DirectReference?.ResourceName ?? "None",
            RefMode.Named => string.IsNullOrEmpty(NamedReference) ? "None" : NamedReference,
        };
    }
    
    public record AnimationRef : ResourceRef<AnimationClip> {}
    
    public record BoneMaskRef : ResourceRef<BoneMask> {}
}
