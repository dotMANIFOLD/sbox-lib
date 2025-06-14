using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using MANIFOLD.AnimGraph.Jobs;
using MANIFOLD.Inspector;
using Sandbox;

namespace MANIFOLD.AnimGraph.Nodes {
    [Category(JobCategories.SAMPLING)]
    [ExposeToAnimGraph]
    public class AnimationClip : JobNode {
        [Animation]
        public string Animation { get; set; }
        [Title("Playback Speed")]
        public ParameterRef<float> PlaybackSpeedParameter { get; set; } = new();
        [Title("Fallback Value"), HideIfValid(nameof(PlaybackSpeedParameter))]
        public float PlaybackSpeed { get; set; } = 1;
        public bool Looping { get; set; }

        
        [JsonIgnore, Hide]
        public override string DisplayName => "Animation Clip";
        [JsonIgnore, Hide]
        public override Color AccentColor => JobCategories.SAMPLING_COLOR;

        public AnimationClip() {
            
        }
        
        public override IBaseAnimJob CreateJob(in JobCreationContext ctx) {
            var job = new SampleJob(ID);
            job.Animation = Animation;
            if (PlaybackSpeedParameter.IsValid()) {
                job.PlaybackSpeedParameter = ctx.parameters.Get<float>(PlaybackSpeedParameter.ID.Value);
            } else {
                job.PlaybackSpeed = PlaybackSpeed;
            }
            job.Looping = Looping;
            return job;
        }

        public override IEnumerable<NodeRef> GetInputs() {
            return Enumerable.Empty<NodeRef>();
        }
    }
}
