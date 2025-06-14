using MANIFOLD.Animation;

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
        public override Parameter Clone() {
            return new FloatParameter() {
                ID = ID,
                Name = Name,
                AutoReset = AutoReset,
                backingField = backingField,
                DefaultValue = DefaultValue
            };
        }
    }

    [ExposeToAnimGraph(Color = "#dfca1f")]
    [Title("Int")]
    public class IntParameter : Parameter<int> {
        public override Parameter Clone() {
            return new IntParameter() {
                ID = ID,
                Name = Name,
                AutoReset = AutoReset,
                backingField = backingField,
                DefaultValue = DefaultValue
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

    [ExposeToAnimGraph(Color = AnimationCollection.BG_COLOR)]
    public class AnimGraphParameter : Parameter<AnimGraph> {
        public override Parameter Clone() {
            return new AnimGraphParameter() {
                ID = ID,
                Name = Name,
                AutoReset = AutoReset,
                backingField = backingField,
                DefaultValue = DefaultValue
            };
        }
    }
}
