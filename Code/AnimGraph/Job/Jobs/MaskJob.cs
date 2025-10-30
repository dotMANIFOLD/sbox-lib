using System;
using MANIFOLD.Jobs;
using Sandbox;

namespace MANIFOLD.AnimGraph.Jobs {
    public class MaskJob : AnimModifierJob {
        public enum TransformSpace {
            /// <summary>
            /// Bones are overriden in local space, relative to their parents.
            /// </summary>
            [Title("Local Space")]
            Local,
            /// <summary>
            /// Bones are overriden in model space, relative to the model.
            /// </summary>
            [Title("Model Space")]
            Model,
        }

        [Flags]
        public enum TransformMask : int {
            None = 0,
            Position = 1,
            Rotation = 1 << 1,
            Scale = 1 << 2,
            All = Position | Rotation | Scale,
        }
        
        private Pose workingPose;
        private Parameter<float> blendParameter;
        private float blendFactor;
        
        public WeightList Weights { get; set; }
        public TransformSpace Space { get; set; } = TransformSpace.Local;
        public TransformMask Mask { get; set; } = TransformMask.All;

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
            var basePose = Inputs[0].Job?.OutputData?.Pose ?? BindData.bindPose;
            var maskedPose = Inputs[1].Job?.OutputData?.Pose ?? BindData.bindPose;
            
            workingPose.CopyFrom(basePose);
            if (Weights != null && !Blend.AlmostEqual(0)) {
                workingPose.Transform(maskedPose, (name, original, other) => {
                    var weight = Weights[name] * Blend;
                    if (weight.AlmostEqual(0)) return;

                    Transform from = Space switch {
                        TransformSpace.Local => original.LocalTransform,
                        TransformSpace.Model => original.ModelTransform,
                        _ => default
                    };
                    Transform to = Space switch {
                        TransformSpace.Local => other.LocalTransform,
                        TransformSpace.Model => other.ModelTransform,
                        _ => default
                    };
                    
                    Transform target = default;
                    target.Position = Mask.HasFlag(TransformMask.Position) ? to.Position : from.Position;
                    target.Rotation = Mask.HasFlag(TransformMask.Rotation) ? to.Rotation : from.Rotation;
                    target.Scale = Mask.HasFlag(TransformMask.Scale) ? to.Scale : from.Scale;

                    Transform result = from.LerpTo(target, weight);

                    switch (Space) {
                        case TransformSpace.Local: {
                            original.LocalTransform = result;
                            break;
                        }
                        case TransformSpace.Model: {
                            original.ModelTransform = result;
                            break;
                        }
                        default: {
                            Log.Error($"No handler for space {Space}!");
                            break;
                        }
                    }
                });
            }

            CreateOutput(workingPose);
        }
    }
}
