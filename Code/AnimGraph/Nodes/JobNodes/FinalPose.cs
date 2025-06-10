using System;

namespace MANIFOLD.AnimGraph.Nodes {
    public sealed class FinalPose : JobNode {
        [Input]
        public JobNodeReference Pose { get; set; }
        
        public override string DisplayName => "Final Pose";

        public FinalPose() {
            ID = Guid.AllBitsSet;
        }

        public override IBaseAnimJob CreateJob() {
            return new ApplyToModelJob();
        }
    }
}
