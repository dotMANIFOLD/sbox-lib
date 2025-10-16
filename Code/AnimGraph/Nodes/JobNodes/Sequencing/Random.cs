using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using MANIFOLD.AnimGraph.Jobs;
using Sandbox;

namespace MANIFOLD.AnimGraph.Nodes {
    /// <summary>
    /// Randomly selects an option.
    /// </summary>
    [Category(JobCategories.SEQUENCING)]
    [ExposeToAnimGraph]
    public class Random : JobNode {
        public class Option : INodeRefProvider {
            [ReadOnly]
            public NodeRef Input { get; set; } = new NodeRef();
            public float Weight { get; set; } = 1;
            
            [Hide, JsonIgnore]
            NodeRef INodeRefProvider.Reference => Input;
            [JsonIgnore, Hide]
            string INodeRefProvider.RefFieldName => nameof(Input);
        }
        
        [Input, WideMode, InlineEditor]
        public Option[] Options { get; set; } = new Option[0];

        public bool UseWeights { get; set; } = false;
        public bool AllowRepeats { get; set; } = true;

        [Hide, JsonIgnore]
        public override string DisplayName => "Random";
        [Hide, JsonIgnore]
        public override Color AccentColor => JobCategories.SEQUENCING_COLOR;
        
        public override IBaseAnimJob CreateJob(JobCreationContext ctx) {
            var job = new RandomJob(ID, Options.Length);
            job.UseRandomWeights = UseWeights;
            if (UseWeights) {
                for (int i = 0; i < Options.Length; i++) {
                    job.RandomWeights[i] = Options[i].Weight;
                }
            }
            return job;
        }

        public override IEnumerable<NodeRef> GetInputs() {
            return Options.Select(x => x.Input);
        }
    }
}
