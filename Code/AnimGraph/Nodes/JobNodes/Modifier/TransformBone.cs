using System.Collections.Generic;
using System.Text.Json.Serialization;
using MANIFOLD.AnimGraph.Jobs;
using Sandbox;

namespace MANIFOLD.AnimGraph.Nodes {
    /// <summary>
    /// Tranforms selected bones.
    /// </summary>
    [Category(JobCategories.MODIFIER)]
    [ExposeToAnimGraph]
    public class TransformBone : JobNode {
        [Input]
        public NodeRef Input { get; set; }
        
        /// <summary>
        /// Bone to be transformed.
        /// </summary>
        [Bone]
        public string TargetBone { get; set; }
        /// <summary>
        /// These bones will be parented to the Target Bone.
        /// </summary>
        [Bone]
        public string[] Followers { get; set; }
        /// <summary>
        /// Which space to modify the Target Bone in.
        /// </summary>
        public TransformBoneJob.TransformSpace Space { get; set; }
        public ParameterRef<Transform> Parameter { get; set; }

        [Hide, JsonIgnore]
        public override string DisplayName => "Transform Bone";
        [Hide, JsonIgnore]
        public override Color AccentColor => JobCategories.MODIFIER_COLOR;
        
        public override IBaseAnimJob CreateJob(in JobCreationContext ctx) {
            var job = new TransformBoneJob(ID);
            
            job.TargetBone = TargetBone;
            job.FollowerBones = Followers;
            job.Space = Space;

            if (Parameter.IsValid) {
                job.Parameter = ctx.parameters.Get<Transform>(Parameter.ID.Value);
            }

            return job;
        }

        public override IEnumerable<NodeRef> GetInputs() {
            return [Input];
        }
    }
}
