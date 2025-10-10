using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MANIFOLD.Jobs;
using Sandbox;

namespace MANIFOLD.AnimGraph.Jobs {
    public class AnimGraphJob : AnimJob, IOrderedJobGraph {
        private readonly AnimGraph graph;
        private readonly OrderedJobGroup mainGroup;
        private List<IJob> jobsEnumerable;
        private Dictionary<string, IBaseAnimJob> accessDictionary;
        
        public AnimGraph AnimGraph => graph;
        public bool IsValid => mainGroup != null;
        
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

            accessDictionary = new Dictionary<string, IBaseAnimJob>();
            foreach (var node in graph.Nodes.Values) {
                if (node.Reachable && node.Accessible) {
                    accessDictionary.Add(node.AccessString, jobs[node.ID]);
                }
            }
            
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

            mainGroup = new OrderedJobGroup();
            jobsEnumerable = new List<IJob>() { mainGroup, this };
            
            var branches = this.ResolveBranchesFlat(false);
            foreach (var level in branches.GroupBy(x => x.depth).OrderByDescending(x => x.Key)) {
                JobGroup group = null;
                if (level.Count() > 1) group = new JobGroup().SetGraph(mainGroup);
                foreach (var branch in level) {
                    if (branch.jobs.Count > 1) {
                        branch.CreateGraph<OrderedJobGroup>().SetGraph(group ?? mainGroup);
                    } else {
                        branch.jobs[0].SetGraph(group ?? mainGroup);
                    }
                }
            }
        }
        
        public override void Run() {
            mainGroup.Run();
            
            OutputData = Inputs[0].Job.OutputData;
        }

        public void AddJob(IJob job) {
            
        }

        public void RemoveJob(IJob job) {
            
        }

        public IJob GetJob(Guid id) {
            return null;
        }

        public IBaseAnimJob GetAccessibleJob(string str) {
            if (accessDictionary.TryGetValue(str, out IBaseAnimJob job)) {
                return job;
            }
            return null;
        }
        
        public IEnumerator<IJob> GetEnumerator() {
            return jobsEnumerable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
