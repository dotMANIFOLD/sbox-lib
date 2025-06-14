using MANIFOLD.Animation;

namespace MANIFOLD.AnimGraph.Parameters {
    [ExposeToAnimGraph(Color = "#2b5bcb")]
    [Title("Bool")]
    public class BoolParameter : Parameter<bool> {}
    [ExposeToAnimGraph(Color = "#afd61f")]
    [Title("Float")]
    public class FloatParameter : Parameter<float> {}
    [ExposeToAnimGraph(Color = "#dfca1f")]
    [Title("Int")]
    public class IntParameter : Parameter<int> {}
    [ExposeToAnimGraph(Color = "#249a1e")]
    [Title("Vector")]
    public class VectorParameter : Parameter<Vector3> {}
    [ExposeToAnimGraph(Color = "#ce1c5d")]
    public class RotationParameter : Parameter<Rotation> {}
    [ExposeToAnimGraph(Color = AnimationCollection.BG_COLOR)]
    public class AnimGraphParameter : Parameter<AnimGraph> {}
}
