using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using MANIFOLD.Animation;
using Sandbox;

namespace MANIFOLD.AnimGraph {
    using Nodes;
    
    [GameResource("Animation Graph", EXTENSION, "Graph to animate a model", Category = LibraryData.CATEGORY, Icon = "account_tree", IconBgColor = AnimationCollection.BG_COLOR)]
    public class AnimGraph : GameResource {
        public const string EXTENSION = "manmgrph";
        public const string TYPE_FIELD = "_type";
        
        public AnimationCollection Collection { get; set; }
        [WideMode, ReadOnly, JsonIgnore]
        public Dictionary<Guid, JobNode> Nodes { get; set; }
        [WideMode, ReadOnly, JsonIgnore]
        public Dictionary<Guid, Parameter> Parameters { get; set; }
        
        [Hide]
        public JsonArray SerializedNodes {
            get {
                JsonArray arr = new JsonArray();
                foreach (var node in Nodes.Values) {
                    var jsonNode = Json.ToNode(node);
                    jsonNode[TYPE_FIELD] = Json.ToNode(node.GetType(), typeof(Type));
                    arr.Add(jsonNode);
                }
                return arr;
            }
            set {
                Nodes.Clear();
                foreach (var node in value) {
                    var type = Json.FromNode<Type>(node[TYPE_FIELD]);
                    var deserialized = (JobNode)Json.Deserialize(node.ToString(), type);
                    deserialized.Graph = this;
                    Nodes.Add(deserialized.ID, deserialized);
                }
            }
        }
        
        [Hide]
        public JsonArray SerializedParameters {
            get {
                JsonArray arr = new JsonArray();
                foreach (var param in Parameters.Values) {
                    var jsonNode = Json.ToNode(param);
                    jsonNode[TYPE_FIELD] = Json.ToNode(param.GetType(), typeof(Type));
                    arr.Add(jsonNode);
                }
                return arr;
            }
            set {
                Parameters.Clear();
                foreach (var node in value) {
                    var type = Json.FromNode<Type>(node[TYPE_FIELD]);
                    var deserialized = (Parameter)Json.Deserialize(node.ToString(), type);
                    Parameters.Add(deserialized.ID, deserialized);
                }
            }
        }
        
        [JsonIgnore, Hide]
        public FinalPose FinalPoseNode => (FinalPose)Nodes[Guid.AllBitsSet];
        
        public static AnimGraph DefaultPreset() {
            var graph = new AnimGraph();
            graph.AddNode(new AnimationClip() { Position = new Vector2(-500, 0) });
            return graph;
        }
        
        public AnimGraph() {
            Nodes = new Dictionary<Guid, JobNode>();
            Parameters = new Dictionary<Guid, Parameter>();
            Nodes.Add(Guid.AllBitsSet, new FinalPose());
        }
        
        public void AddNode(JobNode node) {
            if (Nodes.ContainsKey(node.ID)) {
                Log.Warning($"Node with ID {node.ID} already exists!");
                return;
            }
            node.Graph = this;
            Nodes.Add(node.ID, node);
        }

        public void RemoveNode(JobNode node) {
            node.Graph = null;
            Nodes.Remove(node.ID);
        }

        public void RemoveNode(Guid id) {
            Nodes.Remove(id);
        }
    }
}
