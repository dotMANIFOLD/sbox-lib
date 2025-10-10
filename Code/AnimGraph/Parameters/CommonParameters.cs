using Sandbox;

namespace MANIFOLD.AnimGraph.Parameters {
    [ExposeToAnimGraph(Color = "#2b5bcb")]
    [Title("Bool")]
    public class BoolParameter : Parameter<bool> {
        public override Parameter Clone() {
            return new BoolParameter() {
                ID = ID,
                Name = Name,
                AutoReset = AutoReset,
                backingField = backingField,
                DefaultValue = DefaultValue
            };
        }
    }

    [ExposeToAnimGraph(Color = "#afd61f")]
    [Title("Float")]
    public class FloatParameter : Parameter<float> {
	    [Order(1000)]
	    public bool HasRange { get; set; }
		[Title("Minimum"), ShowIf(nameof(HasRange), true), Order(1001)]
	    public float MinValue { get; set; }
		[Title("Maximum"), ShowIf(nameof(HasRange), true), Order(1002)]
	    public float MaxValue { get; set; }

	    public override float Value {
		    get => base.Value;
		    set {
			    if (HasRange) {
                    value = value.Clamp(MinValue, MaxValue);
                }
                base.Value = value;
		    }
	    }

	    public override Parameter Clone() {
            return new FloatParameter() {
                ID = ID,
                Name = Name,
                AutoReset = AutoReset,
                backingField = backingField,
                DefaultValue = DefaultValue,
                HasRange = HasRange,
                MinValue = MinValue,
                MaxValue = MaxValue,
            };
        }
    }

    [ExposeToAnimGraph(Color = "#dfca1f")]
    [Title("Int")]
    public class IntParameter : Parameter<int> {
        [Order(1000)]
        public bool HasRange { get; set; }
        [Title("Minimum"), ShowIf(nameof(HasRange), true), Order(1001)]
        public int MinValue { get; set; }
        [Title("Maximum"), ShowIf(nameof(HasRange), true), Order(1002)]
        public int MaxValue { get; set; }

        public override int Value {
            get => base.Value;
            set {
                if (HasRange) {
                    value = value.Clamp(MinValue, MaxValue);
                }
                base.Value = value;
            }
        }
        
        public override Parameter Clone() {
            return new IntParameter() {
                ID = ID,
                Name = Name,
                AutoReset = AutoReset,
                backingField = backingField,
                DefaultValue = DefaultValue,
                HasRange = HasRange,
                MinValue = MinValue,
                MaxValue = MaxValue,
            };
        }
    }
    
    [ExposeToAnimGraph(Color = "#249a1e")]
    [Title("Vector")]
    public class VectorParameter : Parameter<Vector3> {
        public override Parameter Clone() {
            return new VectorParameter() {
                ID = ID,
                Name = Name,
                AutoReset = AutoReset,
                backingField = backingField,
                DefaultValue = DefaultValue
            };
        }
    }

    [ExposeToAnimGraph(Color = "#ce1c5d")]
    public class RotationParameter : Parameter<Rotation> {
        public override Parameter Clone() {
            return new RotationParameter() {
                ID = ID,
                Name = Name,
                AutoReset = AutoReset,
                backingField = backingField,
                DefaultValue = DefaultValue
            };
        }
    }
}
