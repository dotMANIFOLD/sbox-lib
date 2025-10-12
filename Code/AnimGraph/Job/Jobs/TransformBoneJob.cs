using System;
using System.Collections.Generic;
using System.Linq;
using MANIFOLD.Jobs;

namespace MANIFOLD.AnimGraph.Jobs {
    public class TransformBoneJob : AnimJob {
        public enum TransformSpace {
            /// <summary>
            /// Relative to the target bone.
            /// </summary>
            Pivot,
            /// <summary>
            /// Relative to the parent.
            /// </summary>
            Local,
            /// <summary>
            /// Relative to the model.
            /// </summary>
            Model
        }

        private Pose workingPose;
        
        private int targetBoneId;
        private string targetBoneName;
        
        private int[] followerBoneIds;
        private IReadOnlyList<string> followerBoneNames;
        private Transform[] followerTransformCache;
        
        public TransformSpace Space { get; set; }

        public string TargetBone {
            get => targetBoneName;
            set {
                targetBoneName = value;
                CacheBoneIds();
            }
        }

        public IReadOnlyList<string> FollowerBones {
            get => followerBoneNames;
            set {
                followerBoneNames = value;
                CacheBoneIds();
            }
        }
        
        public Parameter<Transform> Parameter { get; set; }

        public TransformBoneJob() : base() {
            inputs = new Input<JobResults>[1];
            targetBoneId = -1;
        }

        public TransformBoneJob(Guid id) : base(id) {
            inputs = new Input<JobResults>[1];
            targetBoneId = -1;
        }
        
        public override void Bind() {
            workingPose = BindData.bindPose.Clone();
            CacheBoneIds();
        }

        public override void Run() {
            var baseResults = Inputs[0].Job?.OutputData;
            var basePose = baseResults?.Pose ?? BindData.bindPose;
            
            workingPose.CopyFrom(basePose);

            if (targetBoneId != -1) {
                var boneTransform = workingPose[targetBoneId];

                if (followerBoneIds != null && followerBoneIds.Length > 0) {
                    for (int i = 0; i < followerBoneIds.Length; i++) {
                        int bone = followerBoneIds[i];
                        var followerTransform = workingPose[bone];
                        followerTransformCache[i] = boneTransform.ModelTransform.ToLocal(followerTransform.ModelTransform);
                    }
                }
                
                Transform rhs = Parameter?.Value ?? Transform.Zero;
                switch (Space) {
                    case TransformSpace.Pivot: {
                        boneTransform.LocalTransform = Transform.Concat(boneTransform.LocalTransform, rhs);
                        break;
                    }
                    case TransformSpace.Local: {
                        boneTransform.LocalTransform = rhs;
                        break;
                    }
                    case TransformSpace.Model: {
                        boneTransform.ModelTransform = rhs;
                        break;
                    }
                    default: {
                        Log.Error($"Unhandled TransformSpace: {Space}");
                        break;
                    }
                }
                
                if (followerBoneIds != null && followerBoneIds.Length > 0) {
                    for (int i = 0; i < followerBoneIds.Length; i++) {
                        int bone = followerBoneIds[i];
                        var followerTransform = workingPose[bone];
                        followerTransform.ModelTransform = boneTransform.ModelTransform.ToWorld(followerTransformCache[i]);
                    }
                }
            }

            OutputData = baseResults != null ? baseResults with { Pose = workingPose } : new JobResults(workingPose);
        }

        private void CacheBoneIds() {
            if (BindData == null) return;
            targetBoneId = BindData.remapTable.GetValueOrDefault(targetBoneName, -1);
            followerBoneIds = followerBoneNames?.Select(x => BindData.remapTable.GetValueOrDefault(x, -1)).ToArray();
            followerTransformCache = followerBoneNames != null ? new Transform[followerBoneNames.Count] : null;
        }
    }
}
