using System;
using System.Collections.Generic;
using Editor.NodeEditor;

namespace MANIFOLD.AnimGraph.Editor {
    public class GraphWrapper : IGraph {
        private Dictionary<Guid, GraphNode> nodes;
        
        public AnimGraph Graph { get; }
        
        public IReadOnlyDictionary<Guid, GraphNode> Nodes => nodes;
        IEnumerable<INode> IGraph.Nodes => nodes.Values;
        
        public GraphWrapper(AnimGraph graph) {
            nodes = new Dictionary<Guid, GraphNode>();
            
            Graph = graph;
            RebuildFromGraph();
        }
        
        public void AddNode(INode node) {
            var graphNode = (GraphNode)node;
            var realNode = graphNode.RealNode;
            
            nodes.Add(realNode.ID, graphNode);
            Graph.AddNode(realNode);
            
            Log.Info($"Added node {realNode.ID}");
        }

        public void RemoveNode(INode node) {
            var graphNode = (GraphNode)node;
            var realNode = graphNode.RealNode;
            
            nodes.Remove(realNode.ID);
            Graph.RemoveNode(realNode);
            
            Log.Info($"Removed node {realNode.ID}");
        }

        public string SerializeNodes(IEnumerable<INode> nodes) {
            Log.Info("Serialize called");
            return "";
        }

        public IEnumerable<INode> DeserializeNodes(string serialized) {
            Log.Info("Deserialize called");
            return [];
        }

        public void RebuildFromGraph() {
            foreach (var node in Graph.Nodes.Values) {
                var editorNode = new GraphNode(this, node);
                nodes.Add(node.ID, editorNode);
            }
        }
    }
}
