using System;
using System.Linq;
using MANIFOLD.Jobs;
using Sandbox;

namespace MANIFOLD.AnimGraph {
    // Implementation based off of ozz-animation BlendingJob.
    public abstract class BlendingJob : AnimJob {
        protected float[] weights;

        private Pose workingPose;
        private int blendPasses;

        public bool SyncPlayback { get; set; } = true;
        
        public BlendingJob(int layerCount) : this(Guid.NewGuid(), layerCount) { }

        public BlendingJob(Guid id, int layerCount) : base(id) {
            inputs = new Input<JobResults>[layerCount];
            weights = new float[layerCount];
            weights[0] = 1;
        }
        
        public override void Bind() {
            workingPose = BindData.bindPose.Clone();
            OutputData = new JobResults(workingPose);
        }

        public override void Prepare() {
            if (!SyncPlayback) return;
            
            var samplers = inputs
                .Index()
                .Select(x => {
                    var cast = x.Item.Job as SampleJob;
                    return (job: cast, weight: weights[x.Index]);
                }).ToArray();

            float duration = -1;
            for (int i = 0; i < samplers.Length; i++) {
                var sampler = samplers[i];
                if (samplers[i].job == null) continue;
                if (duration == -1) {
                    duration = sampler.job.Duration * sampler.weight;
                } else {
                    duration = duration.LerpTo(sampler.job.Duration, sampler.weight);
                }
            }
            
            foreach (var sampler in samplers) {
                sampler.job.graphPlaybackSpeed = sampler.job.Duration / duration;
            }
        }

        public override void Run() {
            workingPose.CopyFromUnsafe(BindData.bindPose);
            blendPasses = 0;

            for (int i = 0; i < inputs.Length; i++) {
                var job = inputs[i].Job;
                float weight = weights[i];
                if (weight.AlmostEqual(0) || weight < 0) continue;
                
                Pose inputPose;
                if (job != null) inputPose = job.OutputData.Pose;
                else inputPose = BindData.bindPose;

                if (blendPasses == 0) {
                    workingPose.Transform(inputPose, (current, other) => {
                        return current.LerpTo(other, weight);
                    });
                } else {
                    workingPose.Transform(inputPose, (current, other) => {
                        return current.LerpTo(other, weight);
                    });
                }

                blendPasses++;
            }
        }

        public virtual void SetLayerCount(int count) {
			ArgumentOutOfRangeException.ThrowIfLessThan( count, 1 );

            if (count < inputs.Length) {
                for (int i = inputs.Length - 1; i > count; i--) {
                    this.RemoveInput(i);
                }
            }
            
            Array.Resize(ref inputs, count);
            Array.Resize(ref weights, count);
		}
    }
}
