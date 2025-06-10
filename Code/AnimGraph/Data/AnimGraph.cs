using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Sandbox;

namespace MANIFOLD.AnimGraph {
    using Nodes;
    
    [GameResource("Animation Graph", EXTENSION, "Graph to animate a model", Category = LibraryData.CATEGORY, Icon = "account_tree")]
    public class AnimGraph : GameResource {
        public const string EXTENSION = "manmgrph";
        
        [WideMode, ReadOnly, JsonIgnore]
        public Dictionary<Guid, JobNode> Nodes { get; set; }
        [Hide]
        public JsonArray SerializedNodes {
            get {
                JsonArray arr = new JsonArray();
                foreach (var node in Nodes.Values) {
                    arr.Add(Json.ToNode(node));
                }
                return arr;
            }
            set {
                Nodes.Clear();
                foreach (var node in value) {
                    var deserialized = (JobNode)Json.Deserialize(node.ToString(), Json.FromNode<Type>(node["Type"]));
                    Nodes.Add((Guid)node[nameof(JobNode.ID)], deserialized);
                }
            }
        }
        
        // EDITOR DATA
        public Vector2 EditorPosition { get; set; }
        public float EditorScale { get; set; } = 1;

        [JsonIgnore, Hide]
        public FinalPose FinalPoseNode => (FinalPose)Nodes[Guid.AllBitsSet];
        
        public static AnimGraph DefaultPreset() {
            var graph = new AnimGraph();
            graph.AddNode(new FinalPose());
            graph.AddNode(new AnimationNode() { Position = new Vector2(-500, 0) });
            return graph;
        }
        
        public AnimGraph() {
            Nodes = new Dictionary<Guid, JobNode>();
        }
        
        public void AddNode(JobNode node) {
            if (Nodes.ContainsKey(node.ID)) {
                Log.Warning($"Node with ID {node.ID} already exists!");
                return;
            }
            Nodes.Add(node.ID, node);
        }
    }
}
