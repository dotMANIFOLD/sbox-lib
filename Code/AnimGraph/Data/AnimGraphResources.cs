using System.Collections.Generic;
using MANIFOLD.Animation;
using Sandbox;

namespace MANIFOLD.AnimGraph {
    [AssetType(Name = "Graph Resources", Category = ModuleData.CATEGORY, Extension = EXTENSION)]
    public class AnimGraphResources : GameResource {
        public const string EXTENSION = ModuleData.EXT_PREFIX + "res";
        
        /// <summary>
        /// Used as preview in the editor. Can be null.
        /// </summary>
        public Model Model { get; set; }
        public List<AnimationClip> Animations { get; set; } = new List<AnimationClip>();
        public List<BoneMask> BoneMasks { get; set; } = new List<BoneMask>();

        protected override Bitmap CreateAssetTypeIcon(int width, int height) {
            return CreateSimpleAssetTypeIcon("list_alt", width, height, ModuleData.BG_COLOR);
        }
    }
}
