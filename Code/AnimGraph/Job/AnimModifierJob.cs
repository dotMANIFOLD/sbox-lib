using System;
using MANIFOLD.Jobs;

namespace MANIFOLD.AnimGraph {
    public abstract class AnimModifierJob : AnimJob {
        public bool ResetChild1 { get; set; } = true;
        public bool ResetChild2 { get; set; } = true;

        public AnimModifierJob(Guid id) : base(id) {
            inputs = new Input<JobResults>[2];
        }

        public override void Reset() {
            if (ResetChild1) {
                if (inputs[0].Job is IBaseAnimJob animJob) animJob.Reset();
            }
            if (ResetChild2) {
                if (inputs[1].Job is IBaseAnimJob animJob) animJob.Reset();
            }
        }
    }
}
