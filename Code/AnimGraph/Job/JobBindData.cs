using MANIFOLD.Animation;
using System.Collections.Generic;
using Sandbox;

namespace MANIFOLD.AnimGraph {
    public class JobBindData {
        public readonly AnimationCollection animations;
        public readonly SkinnedModelRenderer target;
        
        public readonly Pose bindPose;
        public IReadOnlyDictionary<string, int> remapTable;

        public JobBindData(AnimationCollection animations, SkinnedModelRenderer target) {
            this.animations = animations;
            this.target = target;

            var data = ModelPersistentData.Get(target.Model);
            remapTable = data.remapTable;
            bindPose = data.bindPose;
        }
    }
}
