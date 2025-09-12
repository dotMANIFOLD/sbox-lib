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
            public TrackInfo(Track track, Widget parent = null) : base(parent) {
                var grid = Layout.Grid();
                grid.Margin = 4;
                grid.VerticalSpacing = 8;

                Layout = grid;

                var trackType = track.GetType().BaseType.GenericTypeArguments[0];
                
                grid.AddCell(0, 0, new Label($"{track.Name} {(string.IsNullOrEmpty(track.TargetBone) ? "" : $"({track.TargetBone})")}"));
                grid.AddCell(1, 0, new Label(trackType.Name));
                grid.AddCell(0, 1, new Label($"Frame count: {track.FrameCount}"));
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
        private Layout tracksLayout;
        
        public AnimationClipInspector(SerializedObject so) : base(so) {
            Layout = Layout.Column();
            Layout.Margin = 8;
            Layout.Spacing = 4;

            if (so.Targets.FirstOrDefault() is not AnimationClip resource) return;
            target = resource;

            sheet = new ControlSheet();
            Layout.Add(sheet);
            sheet.AddObject(so, SheetFilter);

            Layout.Add(new Label.Header("Tracks"));
            tracksLayout = Layout.AddColumn();
            tracksLayout.Margin = new Margin(8, 0);
            tracksLayout.Spacing = 4;
            
            Layout.AddStretchCell();
            
            RebuildTracks();
        }

        private void RebuildTracks() {
            tracksLayout.Clear(true);

            foreach (var track in target.Tracks) {
                tracksLayout.Add(new TrackInfo(track, this));
            }
        }
        
        private bool SheetFilter(SerializedProperty prop) {
            if (prop.HasAttribute<HideAttribute>()) return false;
            if (prop.Name == nameof(AnimationClip.Tracks)) return false;
            return true;
        }
    }
}
