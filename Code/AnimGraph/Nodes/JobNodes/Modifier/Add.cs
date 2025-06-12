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
        public NodeReference Base { get; set; }
        [Input]
        public NodeReference Additive { get; set; }
        
        [JsonIgnore, Hide]
        public override string DisplayName => "Add";
        [JsonIgnore, Hide]
        public override Color AccentColor => JobCategories.MODIFIER_COLOR;
        
        public override IBaseAnimJob CreateJob() {
            throw new System.NotImplementedException();
        }

        public override IEnumerable<NodeReference> GetInputs() {
            throw new System.NotImplementedException();
        }
    }
}
