using System.Collections.Generic;
using System.Text.Json.Serialization;
using MANIFOLD.AnimGraph.Jobs;
using Sandbox;

namespace MANIFOLD.AnimGraph.Nodes {
    /// <summary>
    /// A point to hook new animations jobs in.
    /// </summary>
    [ExposeToAnimGraph]
    public class HookNode : JobNode {
        [Hide, JsonIgnore]
        public override string DisplayName => "Hook Point";
        [Hide, JsonIgnore]
        public override Color AccentColor => JobCategories.HOOK_COLOR;
        
        public override IBaseAnimJob CreateJob(JobCreationContext ctx) {
            return new HookJob(ID);
        }

        public override IEnumerable<NodeRef> GetInputs() {
            return [];
        }
    }
}
