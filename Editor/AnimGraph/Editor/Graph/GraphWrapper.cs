using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Editor.NodeEditor;
using MANIFOLD.Utility;
using Sandbox;

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
            Graph.StateHasChanged();
            
            Log.Info($"Added node {realNode.ID}");
        }

        public void RemoveNode(INode node) {
            var graphNode = (GraphNode)node;
            var realNode = graphNode.RealNode;
            
            nodes.Remove(realNode.ID);
            Graph.RemoveNode(realNode);
            Graph.StateHasChanged();
            
            Log.Info($"Removed node {realNode.ID}");
        }

        public string SerializeNodes(IEnumerable<INode> nodes) {
            return nodes.Select(x => ((GraphNode)x).RealNode).ToArray().SerializePolymorphic().ToString();
        }

        public IEnumerable<INode> DeserializeNodes(string serialized) {
            var arr = (JsonArray)JsonNode.Parse(serialized);
            var jobNodes = arr.DeserializePolymorphic<JobNode>();
            var graphNodes = jobNodes.Select(x => new GraphNode(this, x));
            return graphNodes.ToArray();
        }

        public void RebuildFromGraph() {
            nodes.Clear();
            foreach (var node in Graph.Nodes.Values) {
                var editorNode = new GraphNode(this, node);
                nodes.Add(node.ID, editorNode);
            }
        }

        public void ScanReachableNodes() {
            foreach (var node in Graph.Nodes.Values) {
                node.Reachable = false;
            }
            
            HashSet<JobNode> connected = new HashSet<JobNode>();
            var finalPose = Graph.FinalPoseNode;
            GetAllRecurse(connected, finalPose);
            foreach (var node in connected) {
                node.Reachable = true;
            }
        }

        private void GetAllRecurse(HashSet<JobNode> connected, JobNode current) {
            connected.Add(current);
            foreach (var child in current.GetInputs()) {
                if (!child.IsValid) continue;
                GetAllRecurse(connected, Graph.Nodes[child.ID.Value]);
            }
        }
    }
}
