using System.Collections.Generic;
using System.Text.Json.Serialization;
using Sandbox;

namespace MANIFOLD.AnimGraph.Nodes {
    /// <summary>
    /// A two-dimensional blend.
    /// </summary>
    [Title("2D Blend")]
    [Category(JobCategories.BLEND)]
    [ExposeToAnimGraph]
    public class TwoDimensionBlend : JobNode {
        public class BlendPoint : INodeRefProvider {
            public NodeRef Input { get; set; } = new NodeRef(null);
            public string Name { get; set; } = "Unnamed";
            public Vector2 Value { get; set; }
            
            [JsonIgnore, Hide]
            NodeRef INodeRefProvider.Reference => Input;
        }
        
        [Input, WideMode, InlineEditor]
        public BlendPoint[] Points { get; set; } = new BlendPoint[0];
        
        [JsonIgnore, Hide]
        public override string DisplayName => "2D Blend";
        [JsonIgnore, Hide]
        public override Color AccentColor => JobCategories.BLEND_COLOR;

        public override IBaseAnimJob CreateJob(in JobCreationContext ctx) {
            throw new System.NotImplementedException();
        }

        public override IEnumerable<NodeRef> GetInputs() {
            throw new System.NotImplementedException();
        }
    }
}
