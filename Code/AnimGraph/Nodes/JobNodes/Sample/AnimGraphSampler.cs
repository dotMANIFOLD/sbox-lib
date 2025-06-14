using System.Collections.Generic;
using System.Text.Json.Serialization;
using MANIFOLD.Animation;
using Sandbox;

namespace MANIFOLD.AnimGraph.Nodes {
    [Category(JobCategories.SAMPLING)]
    [ExposeToAnimGraph]
    public class AnimGraphSampler : JobNode {
        public ParameterRef<AnimGraph> Parameter { get; set; } = new();

        [JsonIgnore, Hide]
        public override string DisplayName => "Sample Anim Graph";
        [JsonIgnore, Hide]
        public override Color AccentColor => AnimationCollection.BG_COLOR;
        
        public override IBaseAnimJob CreateJob() {
            throw new System.NotImplementedException();
        }

        public override IEnumerable<NodeRef> GetInputs() {
            throw new System.NotImplementedException();
        }
    }
}
