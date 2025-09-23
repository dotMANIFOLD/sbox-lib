using System;
using System.Collections;
using System.Collections.Generic;
using Sandbox;

namespace MANIFOLD.AnimGraph {
    public class Pose : SkeletonData<Transform> {
        public delegate Transform BoneModifier<T>(T id, Transform current, Transform other);

        public Pose(Pose other) : base(other) { }
        
        public Pose(Transform[] data, IReadOnlyDictionary<string, int> remapTable) : base(data, remapTable) { }
        
        public void Transform(Pose otherPose, BoneModifier<string> transformer) {
            foreach (var bone in remapTable) {
                if (otherPose.remapTable.TryGetValue(bone.Key, out int otherIndex)) {
                    data[bone.Value] = transformer(bone.Key, data[bone.Value], otherPose[otherIndex]);
                }
            }
        }

        public void TransformUnsafe(Pose otherPose, BoneModifier<int> transformer) {
            for (int i = 0; i < data.Length; i++) {
                data[i] = transformer(i, data[i], otherPose[i]);
            }
        }
        
        public void CopyFrom(Pose otherPose) {
            foreach (var bone in remapTable) {
                if (otherPose.remapTable.TryGetValue(bone.Key, out int otherIndex)) {
                    data[bone.Value] = otherPose[otherIndex];
                }
            }
        }

        public void CopyFromUnsafe(Pose otherPose) {
            for (int i = 0; i < data.Length; i++) {
                 data[i] = otherPose[i];
            }
        }
        
        public Pose Clone() {
            return new Pose(this);
        }
    }
}
