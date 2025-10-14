using System;
using System.Collections.Generic;
using MANIFOLD.Jobs;
using Sandbox;

namespace MANIFOLD.AnimGraph.Jobs {
    // Algorithm by Ryan Juckett
    // Analytic Two-Bone IK in 2D: https://www.ryanjuckett.com/analytic-two-bone-ik-in-2d/

    // Reference implementation by Guillaume Blanc
    // ozz_animation ik_two_bone_job.cc: https://github.com/guillaumeblanc/ozz-animation/blob/master/src/animation/runtime/ik_two_bone_job.cc

    /// <summary>
    /// IK for two bones only.
    /// </summary>
    public class IKTwoBoneJob : AnimJob {
        public const float EPSILON = 1e-8f;

        private Pose workingPose;

        private int rootBoneId;
        private string rootBoneName;
        private int midBoneId;
        private string midBoneName;
        private int tipBoneId;
        private string tipBoneName;

        private int targetBoneId;
        private string targetBoneName;
        private int poleBoneId;
        private string poleBoneName;

        public string RootBone {
            get => rootBoneName;
            set {
                rootBoneName = value;
                rootBoneId = rootBoneName != null ? BindData?.remapTable.GetValueOrDefault(rootBoneName, -1) ?? -1 : -1;
            }
        }

        public string MidBone {
            get => midBoneName;
            set {
                midBoneName = value;
                midBoneId = midBoneName != null ? BindData?.remapTable.GetValueOrDefault(midBoneName, -1) ?? -1 : -1;
            }
        }

        public string TipBone {
            get => tipBoneName;
            set {
                tipBoneName = value;
                tipBoneId = tipBoneName != null ? BindData?.remapTable.GetValueOrDefault(tipBoneName, -1) ?? -1 : -1;
            }
        }

        public string TargetBone {
            get => targetBoneName;
            set {
                targetBoneName = value;
                targetBoneId = targetBoneName != null ? BindData?.remapTable.GetValueOrDefault(targetBoneName, -1) ?? -1 : -1;
            }
        }

        public string PoleBone {
            get => poleBoneName;
            set {
                poleBoneName = value;
                poleBoneId = poleBoneName != null ? BindData?.remapTable.GetValueOrDefault(poleBoneName, -1) ?? -1 : -1;
            }
        }

        public Parameter<Transform> TargetParameter { get; set; }
        public Parameter<Vector3> PoleParameter { get; set; }

        public IKTwoBoneJob() : base() {
            inputs = new Input<JobResults>[1];
            rootBoneId = -1;
            midBoneId = -1;
            tipBoneId = -1;
            targetBoneId = -1;
            poleBoneId = -1;
        }

        public IKTwoBoneJob(Guid id) : base(id) {
            inputs = new Input<JobResults>[1];
        }

        public override void Bind() {
            workingPose = BindData.bindPose.Clone();
            rootBoneId = rootBoneName != null ? BindData.remapTable.GetValueOrDefault(rootBoneName, -1) : -1;
            midBoneId = midBoneName != null ? BindData.remapTable.GetValueOrDefault(midBoneName, -1) : -1;
            tipBoneId = tipBoneName != null ? BindData.remapTable.GetValueOrDefault(tipBoneName, -1) : -1;
            targetBoneId = targetBoneName != null ? BindData.remapTable.GetValueOrDefault(targetBoneName, -1) : -1;
            poleBoneId = poleBoneName != null ? BindData.remapTable.GetValueOrDefault(poleBoneName, -1) : -1;
        }

        public override void Run() {
            JobResults baseResults = inputs[0].Job?.OutputData;
            Pose basePose = baseResults?.Pose ?? BindData.bindPose;

            workingPose.CopyFrom(basePose);

            bool isChainValid = rootBoneId != -1 && midBoneId != -1 && tipBoneId != -1;
            if (isChainValid) {
                BoneTransform root = workingPose[RootBone];
                BoneTransform mid = workingPose[MidBone];
                BoneTransform tip = workingPose[TipBone];

                Vector3 aPosition = workingPose[RootBone].ModelTransform.Position;
                Vector3 bPosition = workingPose[MidBone].ModelTransform.Position;
                Vector3 cPosition = workingPose[TipBone].ModelTransform.Position;

                Transform target;
                if (TargetParameter != null) target = TargetParameter.Value.WithScale(1);
                else if (targetBoneId != -1) target = workingPose[targetBoneId].ModelTransform;
                else target = Transform.Zero;

                bool hasPole = PoleParameter != null || PoleBone != null;
                Vector3 pole;
                if (PoleParameter != null) pole = PoleParameter.Value;
                else if (poleBoneId != -1) pole = workingPose[poleBoneId].ModelTransform.Position;
                else pole = Vector3.Zero;
                
                Vector3 ab = bPosition - aPosition;
                Vector3 bc = cPosition - bPosition;
                Vector3 ac = cPosition - aPosition;
                Vector3 at = target.Position - aPosition;

                float abLen = ab.Length;
                float bcLen = bc.Length;
                float acLen = ac.Length;
                float atLen = at.Length;

                float oldAbcAngle = TriangleAngle(acLen, abLen, bcLen);
                float newAbcAngle = TriangleAngle(atLen, abLen, bcLen);

                Vector3 axis = Vector3.Cross(ab, bc);
                if (axis.LengthSquared < EPSILON) {
                    axis = hasPole ? Vector3.Cross(pole - aPosition, bc) : Vector3.Zero;
                    if (axis.LengthSquared < EPSILON) axis = Vector3.Cross(at, bc);
                    if (axis.LengthSquared < EPSILON) axis = Vector3.Up;
                }
                axis = axis.Normal;

                float a = 0.5f * (oldAbcAngle - newAbcAngle);
                float sin = MathF.Sin(a);
                float cos = MathF.Cos(a);
                Rotation deltaR = new Rotation(axis.x * sin, axis.y * sin, axis.z * sin, cos);

                mid.ModelTransform = mid.ModelTransform.WithRotation(deltaR * mid.ModelTransform.Rotation);

                cPosition = tip.ModelTransform.Position;
                ac = cPosition - aPosition;
                root.ModelTransform = root.ModelTransform.WithRotation(Rotation.FromToRotation(ac, at) * root.ModelTransform.Rotation);

                if (hasPole) {
                    float acSqrLen = ac.LengthSquared;
                    if (acSqrLen > 0f) {
                        bPosition = mid.ModelTransform.Position;
                        cPosition = tip.ModelTransform.Position;
                        ab = bPosition - aPosition;
                        ac = cPosition - aPosition;
                        
                        Vector3 acNorm = ac / MathF.Sqrt(acSqrLen);
                        Vector3 ah = pole - aPosition;
                        Vector3 abProj = ab - acNorm * Vector3.Dot(ab, acNorm);
                        Vector3 ahProj = ah - acNorm * Vector3.Dot(ah, acNorm);

                        float maxReach = abLen + bcLen;
                        if (abProj.LengthSquared > (maxReach * maxReach * 0.001f) && ahProj.LengthSquared > 0f) {
                            Rotation hintR = Rotation.FromToRotation(abProj, ahProj);
                            root.ModelTransform = root.ModelTransform.WithRotation(hintR * root.ModelTransform.Rotation);
                        }
                    }
                }

                tip.ModelTransform = tip.ModelTransform.WithRotation(target.Rotation);
            }


            OutputData = baseResults != null ? baseResults with { Pose = workingPose } : new JobResults(workingPose);
        }

        private float TriangleAngle(float aLen, float aLen1, float aLen2) {
            float c = ((aLen1 * aLen1 + aLen2 * aLen2 - aLen * aLen) / (aLen1 * aLen2) / 2.0f).Clamp(-1, 1);
            return MathF.Acos(c);
        }
    }
}
