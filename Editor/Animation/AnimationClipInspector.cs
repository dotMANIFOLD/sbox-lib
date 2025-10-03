using System.Linq;
using Editor;
using Sandbox;
using Sandbox.UI;
using ControlSheet = Editor.ControlSheet;
using Label = Editor.Label;

namespace MANIFOLD.Animation.Editor {
    [Inspector(typeof(AnimationClip))]
    public class AnimationClipInspector : InspectorWidget {
        public class TrackInfo : Widget {
            public TrackInfo(ITrack track, Widget parent = null) : base(parent) {
                var grid = Layout.Grid();
                grid.Margin = 4;
                grid.VerticalSpacing = 8;

                Layout = grid;
                
                grid.AddCell(0, 0, new Label(track.Name));
                grid.AddCell(1, 0, new Label(track.GetType().Name));
                grid.AddCell(0, 1, new Label($"Data Type: {track.DataType.Name}"));
                grid.AddCell(0, 2, new Label($"Frame count: {track.FrameCount}"));
            }

            protected override void OnPaint() {
                var rect = LocalRect;
                rect.Bottom = Theme.RowHeight;
                
                Paint.SetBrushAndPen(Theme.BaseAlt);
                Paint.DrawRect(rect);
            }
        }
        
        private AnimationClip target;

        private ControlSheet sheet;
        private Layout boneTracksLayout;
        private Layout eventTracksLayout;
        
        public AnimationClipInspector(SerializedObject so) : base(so) {
            Layout = Layout.Column();
            Layout.Margin = 8;
            Layout.Spacing = 4;

            if (so.Targets.FirstOrDefault() is not AnimationClip resource) return;
            target = resource;

            sheet = new ControlSheet();
            Layout.Add(sheet);
            sheet.AddObject(so, SheetFilter);

            Layout.Add(new Label.Header("Bone Tracks"));
            boneTracksLayout = Layout.AddColumn();
            boneTracksLayout.Margin = new Margin(8, 0);
            boneTracksLayout.Spacing = 4;
            
            Layout.Add(new Label.Header("Event Tracks"));
            eventTracksLayout = Layout.AddColumn();
            eventTracksLayout.Margin = new Margin(8, 0);
            eventTracksLayout.Spacing = 4;
            
            Layout.AddStretchCell();
            
            RebuildTracks();
        }

        private void RebuildTracks() {
            boneTracksLayout.Clear(true);
            foreach (var track in target.BoneTracks) {
                boneTracksLayout.Add(new TrackInfo(track, this));
            }
            
            eventTracksLayout.Clear(true);
            foreach (var track in target.EventTracks) {
                eventTracksLayout.Add(new TrackInfo(track, this));
            }
        }
        
        private bool SheetFilter(SerializedProperty prop) {
            if (prop.HasAttribute<HideAttribute>()) return false;
            if (prop.Name == nameof(AnimationClip.BoneTracks)) return false;
            return true;
        }
    }
}
