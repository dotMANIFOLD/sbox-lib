using Editor;

namespace MANIFOLD.AnimGraph.Editor {
    public class Preview : Widget {
        private AnimGraph graph;
        private PreviewRenderer renderer;

        public AnimGraph Graph {
            get => graph;
            set {
                graph = value;
                renderer.Graph = graph;
            }
        }
        
        public Preview() {
            Name = "Preview";
            WindowTitle = "Preview";

            Layout = Layout.Column();
            Layout.Add(renderer = new PreviewRenderer(this));
        }
    }
}
