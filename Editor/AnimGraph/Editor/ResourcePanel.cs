using Editor;
using Sandbox;

namespace MANIFOLD.AnimGraph.Editor {
    public class ResourcePanel : Widget {
        private readonly AnimGraphEditor editor;

        private ControlSheet sheet;
        
        public ResourcePanel(AnimGraphEditor editor) : base(editor) {
            this.editor = editor;
            editor.OnGraphReload += OnGraphReload;

            Name = "ResourcePanel";
            WindowTitle = "Resource";
            
            Layout = Layout.Column();
            sheet = new ControlSheet();
            Layout.Add(sheet);
            
            Layout.AddStretchCell();
        }

        private void OnGraphReload() {
            sheet.Clear(true);
            sheet.AddObject(editor.GraphResource.GetSerialized());
        }
    }
}
