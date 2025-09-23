using MANIFOLD.Animation;
using System.Collections.Generic;
using Sandbox;

namespace MANIFOLD.AnimGraph {
    public class JobBindData {
        public readonly AnimGraphResources animations;
        public readonly SkinnedModelRenderer target;
        
        public readonly Pose bindPose;
        public readonly Pose zeroPose;
        public IReadOnlyDictionary<string, int> remapTable;

        public JobBindData(AnimGraphResources animations, SkinnedModelRenderer target) {
            this.animations = animations;
            this.target = target;

            var data = ModelPersistentData.Get(target.Model);
            remapTable = data.remapTable;
            bindPose = data.bindPose;
            zeroPose = data.zeroPose;
        }
    }
}
