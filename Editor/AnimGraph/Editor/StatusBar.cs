using Editor;
using Sandbox;

namespace MANIFOLD.AnimGraph.Editor {
    public class StatusBar : Widget {
        private AnimGraph graph;

        private Layout labelColumn;
        private Label nameLabel;
        private Label pathLabel;
        
        private ControlWidget collectionWidget;

        public AnimGraph Graph {
            get => graph;
            set {
                graph = value;
                UpdateLabels();
                UpdateSheet();
            }
        }
        
        public StatusBar(Widget parent) : base(parent) {
            Name = "StatusBar";

            Layout = Layout.Row();
            Layout.Margin = 4;
            
            labelColumn = Layout.Add(Layout.Column());
            
            labelColumn.Add(nameLabel = new Label.Subtitle(this));
            labelColumn.Add(pathLabel = new Label(this));
            
            Layout.AddStretchCell();
            
            nameLabel.Color = Color.White;
            pathLabel.Color = Color.Gray.WithAlpha(0.8f);
        }

        protected override void OnPaint() {
            base.OnPaint();
            Paint.SetBrushAndPen(Theme.SurfaceBackground);
            Paint.DrawRect(new Rect(0, Size), Theme.ControlRadius);
        }

        private void UpdateLabels() {
            nameLabel.Text = graph.ResourceName;
            pathLabel.Text = graph.ResourcePath;
        }

        private void UpdateSheet() {
            if (collectionWidget != null) {
                collectionWidget.Destroy();
            }
            
            var obj = EditorTypeLibrary.GetSerializedObject(graph);
            var prop = obj.GetProperty(nameof(AnimGraph.Resources));

            collectionWidget = ControlWidget.Create(prop);
            labelColumn.Add(collectionWidget);
        }
    }
}
