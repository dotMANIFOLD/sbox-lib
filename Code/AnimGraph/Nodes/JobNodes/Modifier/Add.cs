using System.Collections.Generic;
using System.Text.Json.Serialization;
using MANIFOLD.AnimGraph.Jobs;
using Sandbox;

namespace MANIFOLD.AnimGraph.Nodes {
    /// <summary>
    /// Adds one pose onto another.
    /// </summary>
    [Category(JobCategories.MODIFIER)]
    [ExposeToAnimGraph]
    public class Add : JobNode {
        [Input]
        public NodeRef Base { get; set; } = new NodeRef();
        [Input]
        public NodeRef Additive { get; set; } = new NodeRef();

        public bool ResetBaseChild { get; set; } = true;
        public bool ResetAdditiveChild { get; set; } = true;
        public bool ApplyInModelSpace { get; set; }

        [JsonIgnore, Hide]
        public override string DisplayName => "Add";
        [JsonIgnore, Hide]
        public override Color AccentColor => JobCategories.MODIFIER_COLOR;
        
        public override IBaseAnimJob CreateJob(JobCreationContext ctx) {
            var job = new AddJob(ID);
            job.ResetChild1 = ResetBaseChild;
            job.ResetChild2 = ResetAdditiveChild;
            return job;
        }

        public override IEnumerable<NodeRef> GetInputs() {
            return [Base, Additive];
        }
    }
}
