using System;
using System.Collections.Generic;
using Editor.NodeEditor;
using MANIFOLD.Jobs;
using Sandbox;

namespace MANIFOLD.JobGraph.Editor {
    public class JobGraphWrapper : IGraph {
        public class GroupEntry {
            public IJob job;
            public IJobGraph jobGraph;
            public int depth;
            public List<GroupEntry> children;

            public GroupEntry(IJob job, IJobGraph jobGraph, int depth) {
                this.job = job;
                this.jobGraph = jobGraph;
                this.depth = depth;
                children = new List<GroupEntry>();
            }
        }
        
        private readonly IJobGraph graph;
        private Dictionary<Guid, JobGraphNode> allNodes;
        private Dictionary<Guid, Color> groupColors;
        private List<JobGraphNode> groupUpdateOrder;
        private GroupEntry rootEntry;
        private int maxDepth;

        public IReadOnlyDictionary<Guid, JobGraphNode> AllNodes => allNodes;
        public IReadOnlyDictionary<Guid, Color> GroupColors => groupColors;
        
        IEnumerable<INode> IGraph.Nodes => allNodes.Values;

        public JobGraphWrapper(IJobGraph graph) {
            this.graph = graph;
            allNodes = new Dictionary<Guid, JobGraphNode>();
            groupColors = new Dictionary<Guid, Color>();
            groupUpdateOrder = new List<JobGraphNode>();
            Rebuild();
            Organize();
        }
        
        public void AddNode(INode node) {
            Log.Warning("You can't add nodes.");
        }

        public void RemoveNode(INode node) {
            Log.Warning("You can't remove nodes.");
        }

        public string SerializeNodes(IEnumerable<INode> nodes) {
            return "";
        }

        public IEnumerable<INode> DeserializeNodes(string serialized) {
            return [];
        }

        public void Organize() {
            
        }

        private void OrganizeInternal(GroupEntry entry, Vector2 position) {
            Vector2 pos = position;
            foreach (var job in entry.jobGraph) {
                
            }
        }
        
        public void GroupUpdate() {
            int zIndex = -1000;
            foreach (var group in groupUpdateOrder) {
                group.Update(zIndex);
                zIndex++;
            }
        }
        
        public void Rebuild() {
            allNodes.Clear();
            groupUpdateOrder.Clear();
            groupColors.Clear();
            
            List<GroupEntry> groups = new List<GroupEntry>();
            
            rootEntry = new GroupEntry(null, graph, 0);
            GetAllChildren(groups, rootEntry);
            groupUpdateOrder.Reverse();

            int maxIndex = Math.Max(maxDepth, 2);
            foreach (var group in groups) {
                float factor = (group.depth - 1) / (float)maxIndex;
                float hue = MathX.Lerp(120, 0f, factor);
                groupColors.Add(group.job.ID, new ColorHsv(hue, 0.8f, 0.8f));
            }
        }
        
        private void GetAllChildren(List<GroupEntry> cache, GroupEntry current) {
            foreach (var job in current.jobGraph) {
                var node = new JobGraphNode(this, job);
                allNodes.Add(job.ID, node);

                if (job is IJobGraph subGraph) {
                    // this job is a group

                    groupUpdateOrder.Add(node);
                    
                    var nextEntry = new GroupEntry(job, subGraph, current.depth + 1);
                    current.children.Add(nextEntry);
                    cache.Add(nextEntry);
                    GetAllChildren(cache, nextEntry);
                }
            }

            maxDepth = Math.Max(maxDepth, current.depth);
        }
    }
}
