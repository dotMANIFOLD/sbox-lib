using System;
using System.Collections;
using System.Collections.Generic;
using Sandbox;

namespace MANIFOLD.AnimGraph {
    public class Pose : SkeletonData<BoneTransform> {
        public delegate void BoneModifier<T>(T id, BoneTransform original, BoneTransform other);

        private readonly Model model;
        
        public Pose(Model model, bool applyBind = true) : base(model) {
            this.model = model;
            CreateTransforms(model.Bones.Root, null, applyBind);
        }

        public Pose(Model model, IReadOnlyDictionary<string, int> remapTable, bool applyBind = true) : base(remapTable) {
            this.model = model;
            CreateTransforms(model.Bones.Root, null, applyBind);
        }
        
        public void Transform(Pose otherPose, BoneModifier<string> transformer) {
            foreach (var bone in remapTable) {
                if (otherPose.remapTable.TryGetValue(bone.Key, out int otherIndex)) {
                    transformer(bone.Key, data[bone.Value], otherPose[otherIndex]);
                }
            }
        }

        public void TransformUnsafe(Pose otherPose, BoneModifier<int> transformer) {
            for (int i = 0; i < data.Length; i++) {
                transformer(i, data[i], otherPose[i]);
            }
        }
        
        public void CopyFrom(Pose otherPose) {
            foreach (var bone in remapTable) {
                if (otherPose.remapTable.TryGetValue(bone.Key, out int otherIndex)) {
                    data[bone.Value].CopyFrom(otherPose[otherIndex]);
                }
            }
        }

        public void CopyFromUnsafe(Pose otherPose) {
            for (int i = 0; i < data.Length; i++) {
                 data[i].CopyFrom(otherPose[i]);
            }
        }
        
        public Pose Clone() {
            var copy = new Pose(model, remapTable, false);
            copy.CopyFrom(this);
            return copy;
        }

        private void CreateTransforms(BoneCollection.Bone bone, BoneTransform parent, bool applyBind = false) {
            var newTransform = new BoneTransform(parent);
            data[bone.Index] = newTransform;
            if (applyBind) {
                newTransform.LocalTransform = bone.LocalTransform;
            }

            foreach (var child in bone.Children) {
                CreateTransforms(child, newTransform);
            }
        }
    }
}
