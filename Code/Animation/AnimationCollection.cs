using System.Collections.Generic;
using Sandbox;

namespace MANIFOLD.Animation {
    [GameResource("Animation Collection", EXTENSION, "Contains all animation data for anim graph", Category = LibraryData.CATEGORY, Icon = "sports_gymnastics", IconBgColor = BG_COLOR)]
    public class AnimationCollection : GameResource {
        public const string EXTENSION = "manm";
        public const string BG_COLOR = "#eba73a";
        
        public Model Model { get; set; }
        
        public List<AnimationClip> Animations { get; set; } = new List<AnimationClip>();
    }
}
