using System.Collections.Generic;
using System.Text.Json.Serialization;
using Sandbox;

namespace MANIFOLD.AnimGraph.Nodes {
    /// <summary>
    /// Iterates through a list of options.
    /// </summary>
    [Category(JobCategories.SEQUENCING)]
    [ExposeToAnimGraph]
    public class Iterator : JobNode {
        [Input]
        public NodeRef[] Options { get; set; } = new NodeRef[0];

        [Hide, JsonIgnore]
        public override string DisplayName => "Iterator";
        [Hide, JsonIgnore]
        public override Color AccentColor => JobCategories.SEQUENCING_COLOR;
        
        public override IBaseAnimJob CreateJob(JobCreationContext ctx) {
            throw new System.NotImplementedException();
        }

        public override IEnumerable<NodeRef> GetInputs() {
            return Options;
        }
    }
}
