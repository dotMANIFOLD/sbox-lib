using System.Linq;

namespace MANIFOLD.AnimGraph.Nodes {
    public class AnimationNode : JobNode {
        public string animation;
        public float playbackSpeed = 1;
        public bool looping;
        
        public override string DisplayName => "Animation Clip";

        public AnimationNode() {
            
        }
        
        public override IBaseAnimJob CreateJob() {
            return new SampleJob(ID) {
                animationName = animation,
                playbackSpeed = playbackSpeed,
                looping = looping
            };
        }
    }
}
