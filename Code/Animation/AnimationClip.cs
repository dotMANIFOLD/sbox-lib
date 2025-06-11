using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Sandbox;

namespace MANIFOLD.Animation {
    public class AnimationClip {
        public const string TYPE_FIELD = "_type";
        
        public string Name { get; set; } = "Animation";
        public float FrameRate { get; set; } = 24;
        public int FrameCount { get; set; } = 0;
        
        [WideMode, JsonIgnore]
        public List<Track> Tracks { get; set; } = new List<Track>();
        
        [Hide]
        public JsonArray SerializedTracks {
            get {
                JsonArray arr = new JsonArray();
                foreach (var track in Tracks) {
                    var jsonNode = Json.ToNode(track);
                    jsonNode[TYPE_FIELD] = Json.ToNode(track.GetType(), typeof(Type));
                    arr.Add(jsonNode);
                }
                return arr;
            }
            set {
                Tracks.Clear();
                foreach (var node in value) {
                    var type = Json.FromNode<Type>(node[TYPE_FIELD]);
                    var deserialized = (Track)Json.Deserialize(node.ToString(), type);
                    Tracks.Add(deserialized);
                }
            }
        }
        
        [JsonIgnore]
        public float Duration => (1 / FrameRate) * FrameCount;
        
        public override string ToString() {
            return string.IsNullOrEmpty(Name) ? "Animation" : Name;
        }
    }
}
