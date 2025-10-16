using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using MANIFOLD.AnimGraph.Jobs;
using Sandbox;

namespace MANIFOLD.AnimGraph.Nodes {
    /// <summary>
    /// Sample a single frame of an animation.
    /// </summary>
    [Category(JobCategories.SAMPLING)]
    [ExposeToAnimGraph]
    public class SingleFrame : JobNode {
        public AnimationRef Clip { get; set; } = new();
        public int Frame { get; set; }

        [Hide, JsonIgnore]
        public override string DisplayName => $"Single Frame: {Clip.Name} // {Frame}";
        [Hide, JsonIgnore]
        public override Color AccentColor => JobCategories.SAMPLING_COLOR;
        
        public override IBaseAnimJob CreateJob(JobCreationContext ctx) {
            var job = new SampleJob(ID);
            
            // CLIP
            switch (Clip.Mode) {
                case ResourceRef.RefMode.Named: {
                    job.Clip = ctx.resources.Animations.FirstOrDefault(x => x.Name == Clip.Name);
                    break;
                }
                case ResourceRef.RefMode.Direct: {
                    job.Clip = Clip.DirectReference;
                    break;
                }
                default: {
                    Log.Warning($"Unhandled ResourceRef mode: {Clip.Mode}");
                    break;
                }
            }

            job.PlaybackSpeed = 0;
            job.Time = (1f / job.Clip.FrameRate) * Frame;
            job.ResetZerosTime = false;
            
            return job;
        }

        public override IEnumerable<NodeRef> GetInputs() {
            return [];
        }
    }
}
