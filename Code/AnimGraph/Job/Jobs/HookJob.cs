using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MANIFOLD.Jobs;

namespace MANIFOLD.AnimGraph.Jobs {
    public class HookJob : BlendingJob, IOrderedJobGraph {
        private JobGroup branchGroup;
        private List<IJob> jobsEnumerable;
        
        private Dictionary<string, int> idToIndex;
        private List<OrderedJobGroup> branchGroups;
        private int activeBranch;

        public bool IsValid => true;

        public HookJob(Guid id) : base(id, 0) {
            branchGroup = new JobGroup();
            jobsEnumerable = new List<IJob>() { branchGroup, this };
            idToIndex = new Dictionary<string, int>();
            branchGroups = new List<OrderedJobGroup>();
            activeBranch = -1;
        }

        public override void Run() {
            branchGroup.Run();
            base.Run();
        }

        public void AddBranch(string id, IOutputAnimJob root) {
            if (idToIndex.ContainsKey(id)) {
                throw new ArgumentException("The given branch id is already in use.");
            }

            var index = branchGroups.Count;
            
            var topGroup = new OrderedJobGroup();
            topGroup.SetGraph(branchGroup);
            root.SetGraph(topGroup);

            if (root is IInputAnimJob input) {
                var branches = input.ResolveBranchesFlat();
                foreach (var level in branches.GroupBy(x => x.Depth).OrderByDescending(x => x.Key)) {
                    JobGroup group = null;
                    if (level.Count() > 1) group = new JobGroup().SetGraph(topGroup);
                    foreach (var branch in level) {
                        if (branch.jobs.Count > 1) {
                            branch.CreateGraph<OrderedJobGroup>().SetGraph(group ?? topGroup);
                        } else {
                            branch.jobs[0].SetGraph(group ?? topGroup);
                        }
                    }
                }
            }
            
            topGroup.BindAnimData(BindData);
            topGroup.SetAnimContext(Context);
            
            SetLayerCount(branchGroups.Count + 1);
            root.OutputTo(this, index);
            
            idToIndex.Add(id, index);
            branchGroups.Add(topGroup);
        }

        public void RemoveBranch(string id) {
            if (!idToIndex.TryGetValue(id, out int index)) return;
            if (activeBranch == index) {
                SetActiveBranch(null);
            }
            
            var group = branchGroups[index];

            group.SetGraph(null);
            SetInput(index, null);
            
            idToIndex.Remove(id);
            branchGroups.Remove(group);
        }

        public void SetActiveBranch(string id) {
            if (activeBranch >= 0) {
                weights[activeBranch] = 0;
            }

            if (idToIndex.TryGetValue(id, out int index)) {
                weights[index] = 1;
                activeBranch = index;
            } else {
                activeBranch = -1;
            }
        }
        
        public void AddJob(IJob job) {
            
        }

        public void RemoveJob(IJob job) {
            
        }

        public IJob GetJob(Guid id) {
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
