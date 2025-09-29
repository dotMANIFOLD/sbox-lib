using System;
using System.Collections.Generic;
using System.Linq;
using Editor;
using Editor.NodeEditor;
using MANIFOLD.Jobs;
using Sandbox;

namespace MANIFOLD.JobGraph.Editor {
    public class JobGraphWrapper : IGraph {
        private readonly IJobGraph graph;
        
        private readonly List<GraphNode> allNodes;
        private readonly Dictionary<Guid, GraphJobNode> jobNodes;
        private readonly Dictionary<IJobGraph, GraphGroupNode> groupNodes;
        
        private NodeGroup rootGroup;
        private int maxDepth;
        private bool hasContainers;

        public IReadOnlyDictionary<Guid, GraphJobNode> JobNodes => jobNodes;
        public IReadOnlyDictionary<IJobGraph, GraphGroupNode> GroupNodes => groupNodes;
        
        IEnumerable<INode> IGraph.Nodes => allNodes;

        public JobGraphWrapper(IJobGraph graph) {
            this.graph = graph;
            
            allNodes = new List<GraphNode>();
            jobNodes = new Dictionary<Guid, GraphJobNode>();
            groupNodes = new Dictionary<IJobGraph, GraphGroupNode>();
            
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
        
        public void GroupUpdate() {
            if (rootGroup == null) return;
            
            if (!hasContainers) {
                AssignContainers(rootGroup);
                hasContainers = true;
            }
            
            int zIndex = -1000;
            foreach (var group in groupNodes.Values.Reverse()) {
                group.Update(zIndex);
                zIndex++;
            }
        }
        
        public void Rebuild() {
            allNodes.Clear();
            jobNodes.Clear();

            maxDepth = 4;
            rootGroup = BuildGroups(graph, 0, null);
            SetColors(rootGroup);
        }
        
        public void Organize() {
            PositionGroups(rootGroup, default);
            hasContainers = false;
        }

        private NodeGroup BuildGroups(IJobGraph graph, int depth, NodeGroup parent) {
            var group = new NodeGroup(graph, depth, parent);

            Guid groupId;
            if (graph is IJob graphJob) {
                groupId = graphJob.ID;
            } else {
                groupId = Guid.NewGuid();
            }
            var groupNode = new GraphGroupNode(this, graph, groupId);
            allNodes.Add(groupNode);
            groupNodes.Add(graph, groupNode);

            NodeLeafGroup leaf = null;

            void AddToLeaf(IJob job) {
                if (leaf == null) {
                    leaf = new NodeLeafGroup();
                    group.children.Add(leaf);
                }
                leaf.jobs.Add(job);

                var node = new GraphJobNode(this, job);
                allNodes.Add(node);
                jobNodes.Add(job.ID, node);
            }
            
            foreach (var job in graph) {
                if (job == graph) {
                    AddToLeaf(job);
                    continue;
                }

                if (job is IJobGraph subGraph) {
                    group.children.Add(BuildGroups(subGraph, depth + 1, group));
                    leaf = null;
                } else {
                    AddToLeaf(job);
                }
            }

            maxDepth = Math.Max(maxDepth, depth);
            return group;
        }

        private void SetColors(NodeGroup group) {
            float factor = group.depth / (float)maxDepth;
            float hue = MathX.Lerp(120, 10, factor);
            groupNodes[group.jobGraph].GroupColor = new ColorHsv(hue, 0.8f, 0.8f);

            foreach (var child in group.children) {
                if (child is NodeGroup childGroup) {
                    SetColors(childGroup);
                }
            }
        }

        private void AssignContainers(NodeGroup group) {
            List<GraphicsItem> items = new List<GraphicsItem>();
            foreach (var child in group.children) {
                if (child is NodeGroup childGroup) {
                    var item = groupNodes[childGroup.jobGraph].UI;
                    if (item != null) {
                        items.Add(item);
                    }
                } else if (child is NodeLeafGroup leafGroup) {
                    var nodes = leafGroup.jobs
                        .Select(x => jobNodes[x.ID].UI)
                        .Where(x => x != null);
                    items.AddRange(nodes);
                }
            }
            groupNodes[group.jobGraph].Containing = items;
            
            foreach (var child in group.children) {
                if (child is NodeGroup childGroup) {
                    AssignContainers(childGroup);
                }
            }
        }
        
        private void PositionGroups(NodeGroup group, Vector2 position) {
            int movementAxis = group.layoutDirection == LayoutDirection.RightToLeft ? 0 : 1;
            int centerAxis = group.layoutDirection == LayoutDirection.RightToLeft ? 1 : 0;
            foreach (var child in group.children) {
                Vector2 childPos = position;
                childPos[centerAxis] -= (child.Size[centerAxis] - group.Size[centerAxis]) * 0.5f;

                if (child is NodeGroup childGroup) {
                    PositionGroups(childGroup, childPos);
                } else if (child is NodeLeafGroup leaf) {
                    PositionNodes(leaf, childPos);
                }

                position[movementAxis] += child.Size[movementAxis] + NodeGroup.SPACING;
            }
        }

        private void PositionNodes(NodeLeafGroup leaf, Vector2 position) {
            foreach (var job in leaf.jobs) {
                var node = jobNodes[job.ID];
                node.Position = position;
                position.x += NodeLeafGroup.NODE_WIDTH + NodeLeafGroup.SPACING;
            }
        }
    }
}
