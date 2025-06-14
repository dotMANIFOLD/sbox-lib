using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using MANIFOLD.AnimGraph.Jobs;
using Sandbox;

namespace MANIFOLD.AnimGraph.Nodes {
    /// <summary>
    /// Receives the final result of the anim graph.
    /// </summary>
    public sealed class FinalPose : JobNode {
        [Input]
        public NodeRef Pose { get; set; }
        
        [JsonIgnore, Hide]
        public override string DisplayName => "Final Pose";
        [JsonIgnore, Hide]
        public override Color AccentColor => JobCategories.FINAL_COLOR;

        public FinalPose() {
            ID = Guid.AllBitsSet;
        }

        public override IBaseAnimJob CreateJob(in JobCreationContext ctx) {
            return new ApplyToModelJob();
        }

        public override IEnumerable<NodeRef> GetInputs() {
            return [Pose];
        }
    }
}
