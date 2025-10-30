using System;
using System.Collections.Generic;
using System.Linq;
using MANIFOLD.Animation;
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

        protected void CreateOutput(Pose pose) {
            var baseOutput = inputs[0].Job?.OutputData ?? new JobResults(null);
            
            var baseEvents = baseOutput.TriggeredEvents;
            var childEvents = inputs[1].Job?.OutputData?.TriggeredEvents;
            List<IEvent> outputEvents;
            if (childEvents is { Count: > 0 }) {
                outputEvents = baseEvents?.Concat(childEvents).ToList() ?? childEvents;
            } else {
                outputEvents = baseEvents;
            }

            OutputData = baseOutput with { Pose = pose, TriggeredEvents = outputEvents };
        }
    }
}
