using System;

namespace MANIFOLD.AnimGraph.Jobs {
    public class IntSelectorJob : TransitionJob {
        private int[] values;
        
        private int state;
        private Parameter<int> sourceParameter;

        public int State {
            get => state;
            set {
                state = value;
                sourceParameter = null;
                OnStateChanged();
            }
        }

        public Parameter<int> Source {
            get => sourceParameter;
            set {
                if (sourceParameter != null) {
                    sourceParameter.OnChanged -= OnStateChanged;
                }
                sourceParameter = value;
                if (sourceParameter != null) {
                    sourceParameter.OnChanged += OnStateChanged;
                }
                OnStateChanged();
            }
        }

        public int[] Values => values;

        public IntSelectorJob(int layerCount) : base(layerCount) {
            values = new int[layerCount];
        }

        public IntSelectorJob(Guid id, int layerCount) : base(id, layerCount) {
            values = new int[layerCount];
        }

        public override void SetLayerCount(int count) {
            base.SetLayerCount(count);
            Array.Resize(ref values, count);
        }

        private void OnStateChanged() {
            int newState = Source?.Value ?? State;
            for (int i = 0; i < values.Length; i++) {
                if (values[i] == newState) {
                    DoTransition(i);
                    return;
                }
            }
        }
    }
}
