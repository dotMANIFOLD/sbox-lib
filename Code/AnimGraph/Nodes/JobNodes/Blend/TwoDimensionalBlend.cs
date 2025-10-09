using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using MANIFOLD.AnimGraph.Jobs;
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
            [JsonIgnore, Hide]
            string INodeRefProvider.RefFieldName => nameof(Input);
        }
        
        [Title("X Parameter")]
        public ParameterRef<float> XParameter { get; set; }
        [Title("Y Parameter")]
        public ParameterRef<float> YParameter { get; set; }
        [Input, WideMode, InlineEditor]
        public BlendPoint[] Points { get; set; } = new BlendPoint[0];
        public bool SyncChildren { get; set; } = true;
        
        [JsonIgnore, Hide]
        public override string DisplayName => "2D Blend";
        [JsonIgnore, Hide]
        public override Color AccentColor => JobCategories.BLEND_COLOR;

        public override IBaseAnimJob CreateJob(in JobCreationContext ctx) {
            var job = new PlanarBlendJob(ID, Points.Select(x => x.Value).ToArray());
            if (XParameter.IsValid) {
                job.XParameter = ctx.parameters.Get<float>(XParameter.ID.Value);
            }
            if (YParameter.IsValid) {
                job.YParameter = ctx.parameters.Get<float>(YParameter.ID.Value);
            }
            job.SyncPlayback = SyncChildren;
            return job;
        }

        public override IEnumerable<NodeRef> GetInputs() {
            return Points.Select(x => x.Input);
        }
    }
}
