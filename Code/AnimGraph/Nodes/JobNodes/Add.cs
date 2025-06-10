namespace MANIFOLD.AnimGraph.Nodes {
    /// <summary>
    /// Adds one pose onto another.
    /// </summary>
    public class Add : JobNode {
        [Input]
        public JobNodeReference Base { get; set; }
        [Input]
        public JobNodeReference Additive { get; set; }
        
        public override string DisplayName => "Add";
        
        public override IBaseAnimJob CreateJob() {
            throw new System.NotImplementedException();
        }
    }
}
