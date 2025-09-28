using System;

namespace MANIFOLD.AnimGraph.Jobs {
    public class RandomJob : TransitionJob {
        [Flags]
        public enum SelectionMethods {
            OnReset = 1,
            OnFinished = 2,
        }
        
        private float[] randomWeights;
        private float[] weightCache;

        public SelectionMethods SelectionMethod { get; set; } = SelectionMethods.OnReset;
        public bool UseRandomWeights { get; set; }
        public float[] RandomWeights { get; set; }
        
        public RandomJob(int layerCount) : base(layerCount) {
            randomWeights = new float[layerCount];
            weightCache = new float[layerCount];
        }

        public RandomJob(Guid id, int layerCount) : base(id, layerCount) {
            randomWeights = new float[layerCount];
            weightCache = new float[layerCount];
        }
        
        public override void Reset() {
            if (SelectionMethod.HasFlag(SelectionMethods.OnReset)) {
                SelectRandom();
            }
        }

        public override void SetLayerCount(int count) {
            base.SetLayerCount(count);
            Array.Resize(ref randomWeights, count);
            Array.Resize(ref weightCache, count);
        }

        public void SelectRandom(bool instant = false) {
            float sum = UseRandomWeights ? randomWeights[0] : 1;
            for (int i = 1; i < weights.Length; i++) {
                float optionWeight = UseRandomWeights ? randomWeights[i] : 1;
                weightCache[i] = sum;
                sum += optionWeight;
            }

            float randResult = Random.Shared.NextSingle() * sum;
            for (int i = 0; i < randomWeights.Length - 1; i++) {
                if (randResult >= weightCache[i] && randResult < weightCache[i + 1]) {
                    DoTransition(i);
                    return;
                }
            }
            
            // do final option
            DoTransition(weights.Length - 1);
        }
    }
}
