using System.Collections.Generic;
using System.Text.Json.Serialization;
using MANIFOLD.AnimGraph.Jobs;
using Sandbox;

namespace MANIFOLD.AnimGraph.Nodes {
    /// <summary>
    /// Twists a chain of bones.
    /// </summary>
    [Category(JobCategories.MODIFIER)]
    [ExposeToAnimGraph]
    public class TwistChain : JobNode {
        [Input]
        public NodeRef Input { get; set; }
        
        [Bone]
        public string[] Chain { get; set; }
        public TwistChainJob.TransformSpace Space { get; set; }
        public ParameterRef<Angles> Parameter { get; set; }
        [Space]
        public Curve PitchCurve { get; set; } = Curve.Linear;
        public Curve YawCurve { get; set; } = Curve.Linear;
        public Curve RollCurve { get; set; } = Curve.Linear;
        
        [Hide, JsonIgnore]
        public override string DisplayName => "Twist Chain";
        [Hide, JsonIgnore]
        public override Color AccentColor => JobCategories.MODIFIER_COLOR;
        
        public override IBaseAnimJob CreateJob(in JobCreationContext ctx) {
            var job = new TwistChainJob(ID);

            job.Bones = Chain;
            job.PitchCurve = PitchCurve;
            job.YawCurve = YawCurve;
            job.RollCurve = RollCurve;
            job.Space = Space;

            if (Parameter.IsValid) {
                job.Parameter = ctx.parameters.Get<Angles>(Parameter.ID.Value);
            }

            return job;
        }

        public override IEnumerable<NodeRef> GetInputs() {
            return [Input];
        }
    }
}
