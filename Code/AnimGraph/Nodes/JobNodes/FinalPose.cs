using System;
using System.Text.Json.Serialization;
using Sandbox;

namespace MANIFOLD.AnimGraph.Nodes {
    /// <summary>
    /// Receives the final result of the anim graph.
    /// </summary>
    public sealed class FinalPose : JobNode {
        [Input]
        public NodeReference Pose { get; set; }
        
        [JsonIgnore, Hide]
        public override string DisplayName => "Final Pose";
        [JsonIgnore, Hide]
        public override Color AccentColor => JobCategories.FINAL_COLOR;

        public FinalPose() {
            ID = Guid.AllBitsSet;
        }

        public override IBaseAnimJob CreateJob() {
            return new ApplyToModelJob();
        }
    }
}
