using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using MANIFOLD.AnimGraph.Jobs;
using Sandbox;

namespace MANIFOLD.AnimGraph.Nodes {
    /// <summary>
    /// A one-dimensional blend.
    /// </summary>
    [Title("1D Blend")]
    [Category(JobCategories.BLEND)]
    [ExposeToAnimGraph]
    public class OneDimensionBlend : JobNode {
        public class BlendPoint : INodeRefProvider {
            [ReadOnly]
            public NodeRef Input { get; set; } = new NodeRef(null);
            public string Name { get; set; } = "Unnamed";
            public float Value { get; set; }
            
            [JsonIgnore, Hide]
            NodeRef INodeRefProvider.Reference => Input;
        }

        public ParameterRef<float> Parameter { get; set; } = new();
        [Input, WideMode, InlineEditor]
        public BlendPoint[] Points { get; set; } = new BlendPoint[0];

        public bool SyncChildren { get; set; } = true;

        [JsonIgnore, Hide]
        public override string DisplayName => "1D Blend";
        [JsonIgnore, Hide]
        public override Color AccentColor => JobCategories.BLEND_COLOR;

        public override IBaseAnimJob CreateJob(in JobCreationContext ctx) {
            var job = new LinearBlendingJob(ID, Points.Select(x => x.Value).ToArray());
            if (Parameter.ID.HasValue) {
                job.BlendParameter = ctx.parameters.Get<float>(Parameter.ID.Value);
            }
            job.SyncPlayback = SyncChildren;
            return job;
        }

        public override IEnumerable<NodeRef> GetInputs() {
            return Points.Select(x => x.Input);
        }
    }
}
