using System;
using Sandbox;

namespace MANIFOLD.AnimGraph.Jobs {
    public class ManualBlendJob : BlendingJob {
        public ManualBlendJob(int layerCount) : base(layerCount) { }
        public ManualBlendJob(Guid id, int layerCount) : base(id, layerCount) { }

        public void SetWeight(int index, float weight) {
            weights[index] = weight.Clamp(0, 1);
        }
    }
}
