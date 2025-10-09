using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using MANIFOLD.Utility;
using Sandbox;

namespace MANIFOLD.Animation {
    [AssetType(Name = "Animation Clip", Category = ModuleData.CATEGORY, Extension = EXTENSION)]
    public class AnimationClip : GameResource, INamedResource {
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
        public List<BoneTrack> BoneTracks { get; set; } = new List<BoneTrack>();
        [Hide, JsonIgnore]
        public List<EventTrack> EventTracks { get; set; } = new List<EventTrack>();
        
        [Hide]
        public JsonArray SerializedBoneTracks {
            get => BoneTracks.SerializePolymorphic();
            set => BoneTracks = value.DeserializePolymorphic<BoneTrack>();
        }

        [Hide]
        public JsonArray SerializedEventTracks {
            get => EventTracks.SerializePolymorphic();
            set => EventTracks = value.DeserializePolymorphic<EventTrack>();
        }

        public void Load() {
            foreach (var track in BoneTracks) {
                if (Game.IsEditor || !track.Loaded) track.Load();
            }
        }

        public void Unload() {
            foreach (var track in BoneTracks) {
                track.Unload();
            }
        }
        
        public void CompressData() {
            foreach (var track in BoneTracks) {
                track.CompressData();
            }
        }
        
        protected override Bitmap CreateAssetTypeIcon(int width, int height) {
            return CreateSimpleAssetTypeIcon("animation", width, height, ModuleData.BG_COLOR);
        }
    }
}
