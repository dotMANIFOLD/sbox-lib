using System;
using System.Collections.Generic;
using System.Linq;
using MANIFOLD.Jobs;
using Sandbox;

namespace MANIFOLD.AnimGraph.Jobs {
    public class TwistChainJob : AnimJob {
        public enum TransformSpace {
            [Title("Local Space")]
            Local,
            [Title("Model Space")]
            Model
        }
        
        private Pose workingPose;

        private int[] boneIds;
        private Vector3[] boneSteps;
        private IReadOnlyList<string> boneNames;

        private Rotation[] rotCache;
        
        public IReadOnlyList<string> Bones {
            get => boneNames;
            set {
                boneNames = value;
                CacheData();
            }
        }
        
        public TransformSpace Space { get; set; }
        
        public Curve PitchCurve { get; set; } = Curve.Linear;
        public Curve YawCurve { get; set; } = Curve.Linear;
        public Curve RollCurve { get; set; } = Curve.Linear;
        
        public Parameter<Angles> Parameter { get; set; }
        
        public TwistChainJob() : base() {
            inputs = new Input<JobResults>[1];
        }

        public TwistChainJob(Guid id) : base(id) {
            inputs = new Input<JobResults>[1];
        }

        public override void Bind() {
            workingPose = BindData.bindPose.Clone();
            CacheData();
        }

        public override void Run() {
            JobResults baseResults = inputs[0].Job?.OutputData;
            Pose basePose = baseResults?.Pose ?? BindData.bindPose;
            
            workingPose.CopyFrom(basePose);
            for (int i = 0; i < boneIds.Length; i++) {
                BoneTransform transform = workingPose[boneIds[i]];
                Rotation rot = Space switch {
                    TransformSpace.Local => transform.LocalTransform.Rotation,
                    TransformSpace.Model => transform.ModelTransform.Rotation,
                    _ => throw new NotImplementedException(),
                };
                rotCache[i] = rot;
            }

            Angles paramValue = Parameter?.Value ?? default;
            for (int i = 0; i < boneIds.Length; i++) {
                BoneTransform transform = workingPose[boneIds[i]];
                Angles angles = new Angles(paramValue.AsVector3() * boneSteps[i]);
                Rotation newRot = angles.ToRotation() * rotCache[i];

                switch (Space) {
                    case TransformSpace.Local: {
                        transform.LocalTransform = transform.LocalTransform.WithRotation(newRot);
                        break;
                    }
                    case TransformSpace.Model: {
                        transform.ModelTransform = transform.ModelTransform.WithRotation(newRot);
                        break;
                    }
                }
            }

            OutputData = baseResults != null ? baseResults with { Pose = workingPose } : new JobResults(workingPose);
        }

        private void CacheData() {
            if (BindData == null) return;
            boneIds = boneNames?
                .Select(x => BindData.remapTable.GetValueOrDefault(x, -1))
                .Where(x => x != -1)
                .ToArray();
            
            boneSteps = new Vector3[boneIds.Length];
            for (int i = 0; i < boneSteps.Length; i++) {
                float factor = (float)i / (boneSteps.Length - 1);
                boneSteps[i].x = PitchCurve.Evaluate(factor);
                boneSteps[i].y = YawCurve.Evaluate(factor);
                boneSteps[i].z = RollCurve.Evaluate(factor);
            }
            
            rotCache = new Rotation[boneIds.Length];
        }
    }
}
