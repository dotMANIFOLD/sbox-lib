using System;
using Sandbox;

namespace MANIFOLD.AnimGraph.Jobs {
    public class TransitionJob : BlendingJob {
        private int activeInput;
        
        private bool inTransition;
        private float transitionRuntime;
        private int transitionMuteIndex;
        private int transitionFadeIndex;
        private bool transitionInvertFade;
        
        public float TransitionDuration { get; set; }
        public Curve TransitionCurve { get; set; }

        public bool ResetOnChange { get; set; } = true;

        public TransitionJob(int layerCount) : base(layerCount) {
            SyncPlayback = false;
            if (layerCount > 0) {
                weights[0] = 1;
            }
        }

        public TransitionJob(Guid id, int layerCount) : base(id, layerCount) {
            SyncPlayback = false;
            if (layerCount > 0) {
                weights[0] = 1;
            }
        }

        public override void Run() {
            if (inTransition) {
                transitionRuntime += Context.deltaTime;

                if (transitionRuntime >= TransitionDuration) {
                    FinishTransition();
                } else {
                    float factor = transitionRuntime / TransitionDuration;
                    float eased = TransitionCurve.Evaluate(factor);
                    if (transitionInvertFade) eased = 1 - eased;
                    
                    weights[transitionFadeIndex] = eased;
                }
            }
            
            base.Run();
        }

        public override void SetLayerCount(int count) {
            FinishTransition();
            base.SetLayerCount(count);
        }

        public virtual void DoTransition(int targetIndex) {
            if (targetIndex == activeInput) return;
            ArgumentOutOfRangeException.ThrowIfNegative(targetIndex, nameof(targetIndex));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(targetIndex, weights.Length, nameof(targetIndex));

            if (inTransition) {
                FinishTransition();
            }
            
            transitionInvertFade = activeInput > targetIndex;
            transitionFadeIndex = transitionInvertFade ? activeInput : targetIndex;
            transitionMuteIndex = activeInput;
            activeInput = targetIndex;

            if (transitionInvertFade) {
                weights[targetIndex] = 1;
            }
            if (ResetOnChange) {
                var targetJob = Inputs[targetIndex].Job;
                if (targetJob != null && targetJob is IAnimJob animJob) {
                    animJob.Reset();
                }
            }
            
            transitionRuntime = 0;
            inTransition = true;
        }

        protected virtual void FinishTransition() {
            inTransition = false;

            if (!transitionInvertFade) {
                weights[transitionFadeIndex] = 1;
            }
            weights[transitionMuteIndex] = 0;
        }
    }
}
