using System.Collections.Generic;
using MANIFOLD.Animation;
using Sandbox;

namespace MANIFOLD.AnimGraph {
    public class WeightList : SkeletonData<float> {
        public WeightList(SkeletonData<float> other) : base(other) { }

        public WeightList(float[] data, IReadOnlyDictionary<string, int> remapTable) : base(data, remapTable) { }

        public WeightList(BoneMask mask, Model model) : base(model) {
            IterateBones(mask, model.Bones.Root, 0);
        }

        private void IterateBones(BoneMask mask, BoneCollection.Bone bone, float defaultWeight) {
            float weight = mask.Weights.GetValueOrDefault(bone.Name, defaultWeight);
            this[bone.Name] = weight;
            
            foreach (var child in bone.Children) {
                IterateBones(mask, child, weight);
            }
        }
    }
}
