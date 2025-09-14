using System;
using System.Linq;
using MANIFOLD.Jobs;
using Sandbox;

namespace MANIFOLD.AnimGraph.Jobs {
    public class AnimGraphJob : AnimJob {
        private readonly AnimGraph graph;
        
        public AnimGraph Graph => graph;
        
        public AnimGraphJob(AnimGraph graph, JobCreationContext ctx) : base(Guid.NewGuid()) {
            this.graph = graph;

            inputs = new Input<JobResults>[1];
            
            var jobs = graph.Nodes.Values
                .Where(x => x.Reachable)
                .Where(x => x.ID != Guid.AllBitsSet) // skip final pose node
                .Select(x => x.CreateJob(ctx))
                .ToDictionary(x => x.ID);

            var finalPose = graph.FinalPoseNode;
            this.InputFrom((IOutputAnimJob)jobs[finalPose.Pose.ID.Value], 0);
            
            foreach (var job in jobs.Values) {
                var animNode = graph.Nodes[job.ID];
                if (animNode == null) {
                    Log.Warning($"Graph node {job.ID} is missing? This shouldn't happen.");
                    continue;
                }
                if (job is not IInputAnimJob inputJob) {
                    continue;
                }

                var inputs = animNode.GetInputs();
                int index = 0;
                foreach (var reference in inputs) {
                    if (!reference.IsValid()) continue;
                    inputJob.InputFrom((IOutputAnimJob)jobs[reference.ID.Value], index);
                    index++;
                }
            }
            
            // Do we group here? or should that only be handled by
            // the one assembling the graph?
        }
        
        public override void Run() {
            OutputData = Inputs[0].Job.OutputData;
        }
    }
}
