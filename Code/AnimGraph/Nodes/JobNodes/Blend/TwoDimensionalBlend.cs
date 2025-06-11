using System.Text.Json.Serialization;
using Sandbox;

namespace MANIFOLD.AnimGraph.Nodes {
    /// <summary>
    /// A two-dimensional blend.
    /// </summary>
    [Category(JobCategories.BLEND)]
    [ExposeToAnimGraph]
    public class TwoDimensionBlend : JobNode {
        public class BlendPoint : INodeReferenceProvider {
            public NodeReference Input { get; set; } = new NodeReference(null);
            public string Name { get; set; } = "Unnamed";
            public Vector2 Value { get; set; }
            
            [JsonIgnore, Hide]
            NodeReference INodeReferenceProvider.Reference => Input;
        }
        
        [Input, WideMode, InlineEditor]
        public BlendPoint[] Points { get; set; } = new BlendPoint[0];
        
        [JsonIgnore, Hide]
        public override string DisplayName => "2D Blend";
        [JsonIgnore, Hide]
        public override Color AccentColor => JobCategories.BLEND_COLOR;

        public override IBaseAnimJob CreateJob() {
            throw new System.NotImplementedException();
        }
    }
}
