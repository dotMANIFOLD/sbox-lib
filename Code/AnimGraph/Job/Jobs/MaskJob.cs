using System;
using MANIFOLD.Jobs;
using Sandbox;

namespace MANIFOLD.AnimGraph.Jobs {
    public class MaskJob : AnimModifierJob {
        private Pose workingPose;
        private Parameter<float> blendParameter;
        private float blendFactor;
        
        public WeightList Weights { get; set; }

        public float Blend {
            get => blendParameter?.Value ?? blendFactor;
            set {
                blendFactor = value.Clamp(0, 1);
                blendParameter = null;
            }
        }

        public Parameter<float> BlendParemeter {
            get => blendParameter;
            set => blendParameter = value;
        }
        
        public MaskJob() : base(Guid.NewGuid()) {
            
        }
        
        public MaskJob(Guid id) : base(id) {
            blendFactor = 1;
        }

        public override void Bind() {
            workingPose = BindData.bindPose.Clone();
        }

        public override void Run() {
            var basePose = Inputs[0].Job?.OutputData.Pose ?? BindData.bindPose;
            var maskedPose = Inputs[1].Job?.OutputData.Pose ?? BindData.bindPose;
            
            workingPose.CopyFrom(basePose);
            if (Weights != null && !Blend.AlmostEqual(0)) {
                workingPose.Transform(maskedPose, (name, original, other) => {
                    var weight = Weights[name] * Blend;
                    if (weight.AlmostEqual(0)) return original;
                    return original.LerpTo(other, weight);
                });
            }

            if (Inputs[0].Job != null) {
                OutputData = Inputs[0].Job.OutputData with { Pose = workingPose };
            } else {
                OutputData = new JobResults(workingPose);
            }
        }
    }
}
