using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using MANIFOLD.AnimGraph.Jobs;
using Sandbox;

namespace MANIFOLD.AnimGraph.Nodes {
    /// <summary>
    /// Apply poses over a selection.
    /// </summary>
    [Category(JobCategories.MODIFIER)]
    [ExposeToAnimGraph]
    public class BoneMask : JobNode {
        [Input]
        public NodeRef Base { get; set; } = new();
        [Input]
        public NodeRef Override { get; set; } = new();
        
        public BoneMaskRef Mask { get; set; } = new();
        public MaskJob.TransformSpace BoneSpace { get; set; } = MaskJob.TransformSpace.Local;
        public MaskJob.TransformMask ComponentMask { get; set; } = MaskJob.TransformMask.All;
        
        /// <summary>
        /// If valid, the mask's weight is multiplied by this parameter.
        /// </summary>
        public ParameterRef<float> BlendSource { get; set; } = new();

        public bool ResetBaseChild { get; set; } = true;
        public bool ResetOverrideChild { get; set; } = true;
        
        [JsonIgnore, Hide]
        public override string DisplayName => "Bone Mask";
        [JsonIgnore, Hide]
        public override Color AccentColor => JobCategories.MODIFIER_COLOR;
        
        public override IBaseAnimJob CreateJob(in JobCreationContext ctx) {
            var job = new MaskJob(ID);
            
            // MASK
            switch (Mask.Mode) {
                case ResourceRef.RefMode.Named: {
                    var mask = ctx.resources.BoneMasks.FirstOrDefault(x => x.Name == Mask.NamedReference);
                    if (mask == null) break;
                    var weightList = new WeightList(mask, ctx.model);
                    job.Weights = weightList;
                    break;
                }
                case ResourceRef.RefMode.Direct: {
                    var mask = Mask.DirectReference;
                    if (mask == null) break;
                    var weightList = new WeightList(mask, ctx.model);
                    job.Weights = weightList;
                    break;
                }
            }
            
            // BLEND
            if (BlendSource.IsValid()) {
                job.BlendParemeter = ctx.parameters.Get<float>(BlendSource.ID.Value);
            }
            
            // OTHER
            job.Space = BoneSpace;
            job.Mask = ComponentMask;
            job.ResetChild1 = ResetBaseChild;
            job.ResetChild2 = ResetOverrideChild;
            
            return job;
        }

        public override IEnumerable<NodeRef> GetInputs() {
            return [Base, Override];
        }
    }
}
