using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace MANIFOLD.AnimGraph.Jobs {
    public class StateMachineJob : TransitionJob {
        public class State {
            public int index;
            public bool end;
            public bool passthrough;
            public bool alwaysEvaluate;
            public IEnumerable<Tag> tags;
            public IReadOnlyList<Transition> transitions;
        }

        public class Transition {
            public State target;
            public bool reset;
            public float duration;
            public Curve curve;
            public IEnumerable<Func<bool>> conditions;

            public bool CanTransition() {
                if (conditions == null) return true;
                
                foreach (var condition in conditions) {
                    var result = condition();
                    if (!result) return false;
                }
                return true;
            }
        }

        private State initialState;
        private State[] alwaysEvaluateStates;
        private State currentState;
        private IEnumerable<IDisposable> tagHandles;
        
        private float timeInState;
        private bool stateFinished;
        private bool machineEnded;
        
        public float TimeInState => timeInState;
        public bool StateFinished => stateFinished;
        
        public StateMachineJob(IReadOnlyList<State> states, State initialState) : this(Guid.NewGuid(), states, initialState) { }

        public StateMachineJob(Guid id, IReadOnlyList<State> states, State initialState) : base(id, states.Count) {
            if (!states.Contains(initialState)) throw new ArgumentException("Initial state must be in the state collection.");
            
            alwaysEvaluateStates = states.Where(s => s.alwaysEvaluate).ToArray();
            this.initialState = initialState;
        }

        public override void Bind() {
            base.Bind();
            SetCurrentState(initialState);
        }

        public override void Run() {
            bool transitioned = false;
            foreach (var state in alwaysEvaluateStates) {
                transitioned |= UpdateState(state);
            }
            if (!transitioned) {
                UpdateState(currentState);
            }

            timeInState += Context.deltaTime;
            
            base.Run();

            OutputData = OutputData with { Finished = machineEnded };
        }

        public override void Reset() {
            currentState = initialState;
            timeInState = 0f;
            stateFinished = false;
            machineEnded = false;
            
            base.Reset();
        }

        private bool UpdateState(State state) {
            var inputResults = Inputs[state.index].Job;
            if (inputResults != null) {
                stateFinished = inputResults.OutputData.Finished;
            }
            
            if (state.transitions == null || state.transitions.Count == 0) return false; 
            
            bool transitioned = false;
            foreach (var transition in state.transitions) {
                transitioned = transition.CanTransition();
                if (transitioned) {
                    DoStateTransition(transition);
                    
                    if (transition.target.passthrough) {
                        UpdateState(transition.target);
                    }
                    break;
                }
            }
            return transitioned;
        }

        private void DoStateTransition(Transition transition) {
            TransitionDuration = transition.duration;
            TransitionCurve = transition.curve;
            ResetOnChange = transition.reset;
            DoTransition(transition.target.index);
            
            SetCurrentState(transition.target);
        }

        private void SetCurrentState(State state) {
            if (tagHandles != null) {
                foreach (var handle in tagHandles) {
                    handle.Dispose();
                }
                tagHandles = null;
            }
            
            currentState = state;
            tagHandles = state.tags?.Select(x => x.CreateHandle()).ToArray();
            timeInState = 0f;
            stateFinished = false;
            machineEnded = state.end;
        }
    }
}
