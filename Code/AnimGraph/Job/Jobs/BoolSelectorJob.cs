using System;

namespace MANIFOLD.AnimGraph.Jobs {
    public class BoolSelectorJob : TransitionJob {
        private bool state;
        private Parameter<bool> sourceParameter;
        private Tag sourceTag;
        private bool suppressChange;

        public bool State {
            get => state;
            set {
                state = value;
                sourceParameter = null;
                sourceTag = null;
                OnStateChanged();
            }
        }

        public Parameter<bool> SourceParameter {
            get => sourceParameter;
            set {
                if (sourceParameter != null) {
                    sourceParameter.OnChanged -= OnStateChanged;
                }
                sourceParameter = value;
                if (sourceParameter != null) {
                    sourceParameter.OnChanged += OnStateChanged;
                }

                suppressChange = true;
                SourceTag = null;
                suppressChange = false;
                
                if (!suppressChange) OnStateChanged();
            }
        }

        public Tag SourceTag {
            get => sourceTag;
            set {
                if (sourceTag != null) {
                    sourceTag.OnStateChanged -= TagCallback;
                }
                sourceTag = value;
                if (sourceTag != null) {
                    sourceTag.OnStateChanged += TagCallback;
                }

                suppressChange = true;
                SourceParameter = null;
                suppressChange = false;
                
                if (!suppressChange) OnStateChanged();
            }
        }
        
        public BoolSelectorJob() : base(2) { }
        public BoolSelectorJob(Guid id) : base(id, 2) { }

        private void OnStateChanged() {
            bool newState = sourceTag?.State ?? sourceParameter?.Value ?? state;
            DoTransition(newState ? 1 : 0);
        }

        private void TagCallback(Tag _) {
            OnStateChanged();
        }
    }
}
