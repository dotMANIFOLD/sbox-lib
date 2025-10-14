using System.Collections.Generic;
using System.Text.Json.Serialization;
using MANIFOLD.AnimGraph.Jobs;
using Sandbox;

namespace MANIFOLD.AnimGraph.Nodes {
    /// <summary>
    /// Simple IK for two joints.
    /// </summary>
    [Category(JobCategories.MODIFIER)]
    [ExposeToAnimGraph]
    public class TwoBoneIK : JobNode {
        public enum ElementType { Bone, Parameter }
        
        public struct TargetData {
            public ElementType Type { get; set; }
            
            [Bone, ShowIf(nameof(Type), ElementType.Bone)]
            public string Bone { get; set; }
            [ShowIf(nameof(Type), ElementType.Parameter)]
            public ParameterRef<Transform> Parameter { get; set; }
        }
        
        public struct PoleData {
            public ElementType Type { get; set; }
            
            [Bone, ShowIf(nameof(Type), ElementType.Bone)]
            public string Bone { get; set; }
            [ShowIf(nameof(Type), ElementType.Parameter)]
            public ParameterRef<Vector3> Parameter { get; set; }
        }
        
        [Input]
        public NodeRef Input { get; set; }
        
        [Bone]
        public string RootBone { get; set; }
        [Bone]
        public string MidBone { get; set; }
        [Bone]
        public string TipBone { get; set; }
        
        [InlineEditor, Space]
        public TargetData Target { get; set; }
        [InlineEditor]
        public PoleData Pole { get; set; }

        [Hide, JsonIgnore]
        public override string DisplayName => "Two Bone IK";
        [Hide, JsonIgnore]
        public override Color AccentColor => JobCategories.MODIFIER_COLOR;
        
        public override IBaseAnimJob CreateJob(in JobCreationContext ctx) {
            var job = new IKTwoBoneJob(ID);

            job.RootBone = RootBone;
            job.MidBone = MidBone;
            job.TipBone = TipBone;
            
            if (Target.Type == ElementType.Parameter && Target.Parameter.IsValid) {
                job.TargetParameter = ctx.parameters.Get<Transform>(Target.Parameter.ID.Value);
            } else if (Target.Type == ElementType.Bone) {
                job.TargetBone = Target.Bone;
            }

            if (Pole.Type == ElementType.Parameter && Pole.Parameter.IsValid) {
                job.PoleParameter = ctx.parameters.Get<Vector3>(Pole.Parameter.ID.Value);
            } else if (Pole.Type == ElementType.Bone) {
                job.PoleBone = Pole.Bone;
            }
            
            return job;
        }

        public override IEnumerable<NodeRef> GetInputs() {
            return [Input];
        }
    }
}
