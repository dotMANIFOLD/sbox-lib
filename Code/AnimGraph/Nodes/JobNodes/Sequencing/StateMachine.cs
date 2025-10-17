using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using MANIFOLD.AnimGraph.Jobs;
using MANIFOLD.Utility;
using Sandbox;

namespace MANIFOLD.AnimGraph.Nodes {
    /// <summary>
    /// A state machine.
    /// </summary>
    [Category(JobCategories.SEQUENCING)]
    [ExposeToAnimGraph]
    public class StateMachine : JobNode {
        [Flags]
        public enum Comparison : int {
            None = 0,
            [Title("==")]
            Equal = 1,
            [Title("!=")]
            NotEqual = 2,
            [Title(">")]
            GreaterThan = 4,
            [Title(">=")]
            GreaterThanOrEqual = 8,
            [Title("<")]
            LessThan = 16,
            [Title("<=")]
            LessThanOrEqual = 32,
            All = ~0
        }
        
        public abstract class Condition {
            public abstract object CurrentMode { get; set; }
            public abstract IReadOnlyDictionary<string, object> AvailableModes { get; }
            
            public abstract Comparison CurrentComparison { get; set; }
            public abstract Comparison SupportedComparisons { get; }
            public abstract object TestValue { get; set; }
            public abstract string TestValueName { get; }
            
            public abstract Color Color { get; }

            public abstract Func<bool> CreateCondition(JobCreationContext ctx, StateMachineJob machine);
        }
        
        public class TimeCondition : Condition {
            public float Time { get; set; }

            [Hide, JsonIgnore]
            public override object CurrentMode {
                get => 0;
                set {
                    
                }
            }
            [Hide, JsonIgnore]
            public override IReadOnlyDictionary<string, object> AvailableModes => new Dictionary<string, object>() {
                { "Time in State", 0 }
            };
            
            public override Comparison CurrentComparison { get; set; } = Comparison.Equal;

            [Hide, JsonIgnore]
            public override Comparison SupportedComparisons => Comparison.All;

            [Hide, JsonIgnore]
            public override object TestValue {
                get => Time;
                set => Time = (float)value;
            }
            [Hide, JsonIgnore]
            public override string TestValueName => nameof(Time);

            [Hide, JsonIgnore]
            public override Color Color => Color.White.Darken(0.15f);

            public override Func<bool> CreateCondition(JobCreationContext ctx, StateMachineJob machine) {
                return CurrentComparison switch {
                    Comparison.Equal => () => Time.AlmostEqual(machine.TimeInState),
                    Comparison.NotEqual => () => !Time.AlmostEqual(machine.TimeInState),
                    Comparison.GreaterThan => () => machine.TimeInState > Time,
                    Comparison.GreaterThanOrEqual => () => machine.TimeInState >= Time,
                    Comparison.LessThan => () => machine.TimeInState < Time,
                    Comparison.LessThanOrEqual => () => machine.TimeInState <= Time,
                    _ => throw new NotImplementedException(),
                };
            }
        }
        
        public class FinishedCondition : Condition {
            public bool State { get; set; } = true;
            public bool Almost { get; set; }

            [Hide, JsonIgnore]
            public override object CurrentMode {
                get => Almost;
                set => Almost = (bool)value;
            }
            [Hide, JsonIgnore]
            public override IReadOnlyDictionary<string, object> AvailableModes => new Dictionary<string, object>() {
                { "On Finished", false },
                { "On Almost Finshed", true },
            };

            public override Comparison CurrentComparison { get; set; } = Comparison.Equal;

            [Hide, JsonIgnore]
            public override Comparison SupportedComparisons => Comparison.Equal | Comparison.NotEqual;

            [Hide, JsonIgnore]
            public override object TestValue {
                get => State;
                set => State = (bool)value;
            }
            [Hide, JsonIgnore]
            public override string TestValueName => nameof(State);

            [Hide, JsonIgnore]
            public override Color Color => "#e83939";

            public override Func<bool> CreateCondition(JobCreationContext ctx, StateMachineJob machine) {
                return CurrentComparison switch {
                    Comparison.Equal => () => State == machine.StateFinished,
                    Comparison.NotEqual => () => State != machine.StateFinished,
                    _ => throw new NotImplementedException(),
                };
            }
        }

        public class ParameterCondition : Condition {
            public Guid? Parameter { get; set; }

            [Hide]
            public JsonNode SerializedTestValue {
                get => TestValue.ToPolymorphic();
                set => TestValue = value.FromPolymorphic<object>();
            }
            
            [Hide, JsonIgnore]
            public override object CurrentMode {
                get => true;
                set {
                    
                }
            }

            [Hide, JsonIgnore] public override IReadOnlyDictionary<string, object> AvailableModes => null;

            public override Comparison CurrentComparison { get; set; } = Comparison.Equal;

            [Hide, JsonIgnore]
            public override Comparison SupportedComparisons => Comparison.All;
            
            [Hide, JsonIgnore]
            public override object TestValue { get; set; }
            [Hide, JsonIgnore]
            public override string TestValueName => nameof(TestValue);
            
            [Hide, JsonIgnore]
            public override Color Color => "#e89139";
            
            public override Func<bool> CreateCondition(JobCreationContext ctx, StateMachineJob machine) {
                if (Parameter == null) return null;
                var parameter = ctx.parameters.Get(Parameter.Value);
                
                return CurrentComparison switch {
                    Comparison.Equal => () => parameter.ObjectValue.Equals(TestValue),
                    Comparison.NotEqual => () => !parameter.ObjectValue.Equals(TestValue),
                    Comparison.GreaterThan => () => ((IComparable)parameter.ObjectValue).CompareTo(TestValue) > 0,
                    Comparison.GreaterThanOrEqual => () => ((IComparable)parameter.ObjectValue).CompareTo(TestValue) >= 0,
                    Comparison.LessThan => () => ((IComparable)parameter.ObjectValue).CompareTo(TestValue) < 0,
                    Comparison.LessThanOrEqual => () => ((IComparable)parameter.ObjectValue).CompareTo(TestValue) <= 0,
                    _ => throw new NotImplementedException(),
                };
            }
        }

        public class TagCondition : Condition {
            public Guid? Tag { get; set; }
            public bool State { get; set; } = true;
            
            [Hide, JsonIgnore]
            public override object CurrentMode {
                get => true;
                set {
                    
                }
            }

            [Hide, JsonIgnore] public override IReadOnlyDictionary<string, object> AvailableModes => null;

            public override Comparison CurrentComparison { get; set; } = Comparison.Equal;

            [Hide, JsonIgnore]
            public override Comparison SupportedComparisons => Comparison.Equal | Comparison.NotEqual;
            
            [Hide, JsonIgnore]
            public override object TestValue {
                get => State;
                set => State = (bool)value;
            }
            [Hide, JsonIgnore]
            public override string TestValueName => nameof(State);
            
            [Hide, JsonIgnore]
            public override Color Color => "#e83985";
            
            public override Func<bool> CreateCondition(JobCreationContext ctx, StateMachineJob machine) {
                if (Tag == null) return null;
                var tag = ctx.tags.Get(Tag.Value);
                
                return CurrentComparison switch {
                    Comparison.Equal => () => tag.State == State,
                    Comparison.NotEqual => () => tag.State != State,
                    _ => throw new NotImplementedException(),
                };
            }
        }
        
        public class Transition {
            public Guid From { get; set; }
            public Guid To { get; set; }

            public float Duration { get; set; } = 0.5f;
            public Curve BlendCurve { get; set; } = Curve.EaseOut;
            public bool ResetDestination { get; set; } = true;
            public bool Disable { get; set; }
            
            [WideMode, Space, JsonIgnore]
            public List<Condition> Conditions { get; set; } = new List<Condition>();

            [Hide]
            public JsonArray SerializedConditions {
                get => Conditions.SerializePolymorphic();
                set => Conditions = value.DeserializePolymorphic<Condition>();
            }
        }
        
        public class State : INodeRefProvider {
            public Guid ID { get; set; } = Guid.NewGuid();
            public string Name { get; set; } = "Unnamed";
            [Hide]
            public NodeRef Input { get; set; }
            
            [Space]
            public bool Start { get; set; }
            public bool End { get; set; }
            public bool PassThrough { get; set; }
            public bool AlwaysEvaluate { get; set; }

            [WideMode, Space]
            public List<TagRef> Tags { get; set; } = new();
            
            [Hide]
            public Vector2 Position { get; set; }
            
            [Hide, JsonIgnore]
            public NodeRef Reference => Input;
            [Hide, JsonIgnore]
            public string RefFieldName => nameof(Input);
        }

        public class Input : INodeRefProvider, INameProvider {
            [Hide, JsonIgnore]
            public State State { get; set; }
            public NodeRef Node { get; set; }

            [Hide, JsonIgnore]
            public NodeRef Reference => Node;
            [Hide, JsonIgnore]
            public string RefFieldName => nameof(Node);
            [Hide, JsonIgnore]
            public string Name => State.Name;
        }
        
        private Input[] inputs;
        
        [Hide, UpdatesInputs]
        public Dictionary<Guid, State> States { get; set; } = new();
        [Hide]
        public List<Transition> Transitions { get; set; } = new();

        [Input]
        public Input[] Inputs {
            get => inputs;
            set {
                inputs = value;
                AssignStatesToInputs();
            }
        }

        [Hide, JsonIgnore]
        public override string DisplayName => "State Machine";
        [Hide, JsonIgnore]
        public override Color AccentColor => JobCategories.SEQUENCING_COLOR;
        
        public override IBaseAnimJob CreateJob(JobCreationContext ctx) {
            var states = States.Values
                .Index()
                .Select(x => (x.Index, x.Item.ID, new StateMachineJob.State() {
                    index = x.Index,
                    end = x.Item.End,
                    passthrough = x.Item.PassThrough,
                    alwaysEvaluate = x.Item.AlwaysEvaluate,
                    tags = x.Item.Tags.Where(tagRef => tagRef.IsValid).Select(tagRef => ctx.tags.Get(tagRef.ID.Value))
                })).ToDictionary(x => x.ID, x => x.Item3);
            StateMachineJob.State initialState = states.First(x => States[x.Key].Start).Value;

            var job = new StateMachineJob(ID, states.Values.ToArray(), initialState);
            
            foreach (var pair in states) {
                pair.Value.transitions = Transitions
                    .Where(x => x.From == pair.Key)
                    .Select(x => new StateMachineJob.Transition() {
                        target = states[x.To],
                        reset = x.ResetDestination,
                        duration = x.Duration,
                        curve = x.BlendCurve,
                        conditions = x.Conditions.Select(x => x.CreateCondition(ctx, job))
                    }).ToArray();
            }

            return job;
        }

        public override IEnumerable<NodeRef> GetInputs() {
            return inputs.Select(x => x.Node);
        }

        public void ResizeArray() {
            Array.Resize(ref inputs, States.Count);
            AssignStatesToInputs();
        }

        private void AssignStatesToInputs() {
            int index = 0;
            foreach (var state in States.Values) {
                if (inputs[index] == null) {
                    inputs[index] = new Input();
                }
                inputs[index].State = state;
                index++;
            }
        }
    }
}
