using System;
using MANIFOLD.Jobs;

namespace MANIFOLD.AnimGraph.Jobs {
    public class AddJob : AnimModifierJob {
        private Pose workingPose;
        
        public AddJob() : base(Guid.NewGuid()) { }
        public AddJob(Guid id) : base(id) { }

        public override void Bind() {
            workingPose = BindData.bindPose.Clone();
        }

        public override void Run() {
            var basePose = Inputs[0].Job?.OutputData.Pose ?? BindData.bindPose;
            var additivePose = Inputs[1].Job?.OutputData.Pose ?? BindData.zeroPose;
            
            workingPose.CopyFrom(basePose);
            workingPose.Transform(additivePose, (_, original, additive) => {
                original.LocalTransform = Transform.Concat(original.LocalTransform, additive.LocalTransform);
            });

            if (Inputs[0].Job != null) {
                OutputData = Inputs[0].Job.OutputData with { Pose = workingPose };
            } else {
                OutputData = new JobResults(workingPose);
            }
        }
    }
}
