using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using MANIFOLD.Animation;
using MANIFOLD.Utility;
using Sandbox;

namespace MANIFOLD.AnimGraph {
    using Nodes;
    
    [AssetType(Name = "Animation Graph", Category = ModuleData.CATEGORY, Extension = EXTENSION)]
    public class AnimGraph : GameResource {
        public const string EXTENSION = ModuleData.EXT_PREFIX + "ph";
        public const string TYPE_FIELD = "_type";
        
        public AnimGraphResources Resources { get; set; }
        [WideMode, ReadOnly, JsonIgnore]
        public Dictionary<Guid, JobNode> Nodes { get; set; }
        [WideMode, ReadOnly, JsonIgnore]
        public Dictionary<Guid, Parameter> Parameters { get; set; }
        
        [Hide]
        public JsonObject SerializedNodes {
            get => Nodes.SerializePolymorphic();
            set {
                Nodes = value.DeserializePolymorphic<Guid, JobNode>((_, node) => {
                    node.Graph = this;
                });
            }
        }
        
        [Hide]
        public JsonObject SerializedParameters {
            get => Parameters.SerializePolymorphic();
            set => Parameters = value.DeserializePolymorphic<Guid, Parameter>();
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

        protected override Bitmap CreateAssetTypeIcon(int width, int height) {
            return CreateSimpleAssetTypeIcon("account_tree", width, height, ModuleData.BG_COLOR);
        }
    }
}
