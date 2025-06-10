using System.Collections.Generic;
using Sandbox;

namespace MANIFOLD.Animation {
    [GameResource("Animation Collection", EXTENSION, "Contains all animation data for anim graph", Category = LibraryData.CATEGORY, Icon = "sports_gymnastics")]
    public class AnimationCollection : GameResource {
        public const string EXTENSION = "manm";
        
        public Model Skeleton { get; set; }
        
        public List<AnimationClip> Animations { get; set; } = new List<AnimationClip>();
    }
}
