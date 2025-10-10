using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using MANIFOLD.AnimGraph.Jobs;
using Sandbox;

namespace MANIFOLD.AnimGraph.Nodes {
    /// <summary>
    /// A manual blend. Most useful when accessed via code.
    /// </summary>
    [Category(JobCategories.BLEND)]
    [ExposeToAnimGraph]
    public class ManualBlend : JobNode {
        public class BlendPoint : INodeRefProvider {
            [ReadOnly]
            public NodeRef Input { get; set; } = new NodeRef(null);
            public string Name { get; set; } = "Unnamed";
            [Range(0, 1)]
            public float Weight { get; set; }
            
            [JsonIgnore, Hide]
            NodeRef INodeRefProvider.Reference => Input;
            [JsonIgnore, Hide]
            string INodeRefProvider.RefFieldName => nameof(Input);
        }
        
        [Input, WideMode, InlineEditor]
        public BlendPoint[] Points { get; set; } = new BlendPoint[0];
        
        public bool SyncChildren { get; set; } = true;
        
        [JsonIgnore, Hide]
        public override string DisplayName => "Manual Blend";
        [JsonIgnore, Hide]
        public override Color AccentColor => JobCategories.BLEND_COLOR;
        
        public override IBaseAnimJob CreateJob(in JobCreationContext ctx) {
            var job = new ManualBlendJob(ID, Points.Length);
            for (int i = 0; i < Points.Length; i++) {
                job.SetWeight(i, Points[i].Weight);
            }

            job.SyncPlayback = SyncChildren;
            return job;
        }

        public override IEnumerable<NodeRef> GetInputs() {
            return Points.Select(x => x.Input);
        }
    }
}
