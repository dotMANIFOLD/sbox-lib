using System.Collections.Generic;
using System.Text.Json.Serialization;
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
        
        public override IBaseAnimJob CreateJob(in JobCreationContext ctx) {
            throw new System.NotImplementedException();
        }

        public override IEnumerable<NodeRef> GetInputs() {
            throw new System.NotImplementedException();
        }
    }
}
