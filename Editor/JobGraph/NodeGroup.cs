using System;
using System.Collections.Generic;
using System.Linq;
using MANIFOLD.Jobs;

namespace MANIFOLD.JobGraph.Editor {
    public enum LayoutDirection { LeftToRight, UpToDown }
    
    public abstract class NodeGroupBase {
        public const float GRAPH_INCREMENT = 12;
        
        public int depth;
        public NodeGroupBase parent;
        public LayoutDirection layoutDirection;
        
        public abstract float Width { get; }
        public abstract float Height { get; }
        public Vector2 Size => new Vector2(Width, Height);
    }

    public class NodeGroup : NodeGroupBase {
        public const float PADDING = GRAPH_INCREMENT * 4;
        public const float SPACING = GRAPH_INCREMENT * 6;
        
        public IJobGraph jobGraph;
        public List<NodeGroupBase> children = new();

        public override float Width {
            get {
                if (layoutDirection == LayoutDirection.LeftToRight) {
                    int spacingCount = Math.Max(0, children.Count - 1);
                    float sum = 0;
                    foreach (var child in children) {
                        sum += child.Width;
                    }
                    sum += SPACING * spacingCount;
                    sum += PADDING * 2;
                    return sum;
                } else if (children.Count > 0) {
                    return children.MaxBy(x => x.Width).Width + (PADDING * 2);
                } else {
                    return PADDING * 2;
                }
            }
        }

        public override float Height {
            get {
                if (layoutDirection == LayoutDirection.UpToDown) {
                    int spacingCount = Math.Max(0, children.Count - 1);
                    float sum = 0;
                    foreach (var child in children) {
                        sum += child.Height;
                    }
                    sum += SPACING * spacingCount;
                    sum += PADDING * 2;
                    return sum;
                } else if (children.Count > 0) {
                    return children.MaxBy(x => x.Height).Height + (PADDING * 2);
                } else {
                    return PADDING * 2;
                }
            }
        }

        public NodeGroup(IJobGraph jobGraph, int depth, NodeGroupBase parent) {
            this.jobGraph = jobGraph;
            this.depth = depth;
            this.parent = parent;

            layoutDirection = jobGraph is IOrderedJobGraph ? LayoutDirection.LeftToRight : LayoutDirection.UpToDown;
        }
    }

    public class NodeLeafGroup : NodeGroupBase {
        public const float NODE_WIDTH = GRAPH_INCREMENT * 13;
        public const float NODE_HEIGHT = GRAPH_INCREMENT * 4;
        public const float SPACING = GRAPH_INCREMENT * 4;
        
        public List<IJob> jobs = new();

        public override float Width {
            get {
                if (layoutDirection == LayoutDirection.LeftToRight) {
                    int spacingCount = Math.Max(0, jobs.Count - 1);
                    return (jobs.Count * NODE_WIDTH) + (spacingCount * SPACING);
                } else {
                    return NODE_WIDTH;
                }
            }
        }

        public override float Height {
            get {
                if (layoutDirection == LayoutDirection.UpToDown) {
                    int spacingCount = Math.Max(0, jobs.Count - 1);
                    return (jobs.Count * NODE_HEIGHT) + (spacingCount * SPACING);
                } else {
                    return NODE_HEIGHT;
                }
            }
        }

        public NodeLeafGroup(NodeGroupBase parent) {
            this.parent = parent;
            layoutDirection = parent?.layoutDirection ?? LayoutDirection.LeftToRight;
        }
    }
}
