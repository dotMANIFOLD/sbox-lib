using System.Linq;
using System.Text.Json.Serialization;
using Sandbox;

namespace MANIFOLD.AnimGraph.Nodes {
    [Category(JobCategories.SAMPLING)]
    [ExposeToAnimGraph]
    public class AnimationClip : JobNode {
        [Animation]
        public string Animation { get; set; }
        public float PlaybackSpeed { get; set; } = 1;
        public bool Looping { get; set; }
        
        [JsonIgnore, Hide]
        public override string DisplayName => "Animation Clip";
        [JsonIgnore, Hide]
        public override Color AccentColor => JobCategories.SAMPLING_COLOR;

        public AnimationClip() {
            
        }
        
        public override IBaseAnimJob CreateJob() {
            return new SampleJob(ID) {
                animationName = Animation,
                playbackSpeed = PlaybackSpeed,
                looping = Looping
            };
        }
    }
}
