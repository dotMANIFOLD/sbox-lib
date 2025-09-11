using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using MANIFOLD.Utility;
using Sandbox;

namespace MANIFOLD.Animation {
    [AssetType(Name = "Animation Clip", Category = ModuleData.CATEGORY, Extension = EXTENSION)]
    public class AnimationClip : GameResource {
        public const string EXTENSION = ModuleData.EXT_PREFIX + "clip";
        
        [ReadOnly]
        public string Name { get; set; } = "Animation";
        [ReadOnly]
        public float FrameRate { get; set; } = 24;
        [ReadOnly]
        public int FrameCount { get; set; } = 0;
        
        /// <summary>
        /// Used to check this was created by hand in the editor. Gives a warning if false.
        /// </summary>
        [Hide, ReadOnly]
        public bool Generated { get; set; }
        [JsonIgnore]
        public float Duration => (1 / FrameRate) * FrameCount;
        
        [Hide, JsonIgnore]
        public List<Track> Tracks { get; set; } = new List<Track>();
        
        [Hide]
        public JsonArray SerializedTracks {
            get => Tracks.SerializePolymorphic();
            set => Tracks = value.DeserializePolymorphic<Track>();
        }

        protected override Bitmap CreateAssetTypeIcon(int width, int height) {
            return CreateSimpleAssetTypeIcon("animation", width, height, ModuleData.BG_COLOR);
        }
    }
}
