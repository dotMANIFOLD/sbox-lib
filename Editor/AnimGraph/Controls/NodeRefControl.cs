using Editor;
using Sandbox;

namespace MANIFOLD.AnimGraph.Editor {
    [CustomEditor(typeof(NodeRef))]
    public class NodeRefControl : ControlWidget {
        public NodeRefControl(SerializedProperty property) : base(property) {
            Layout = Layout.Column();
            Layout.Margin = 4;

            var self = property.GetValue<NodeRef>();
            var label = Layout.Add(new Label());
            label.Bind("Text").ReadOnly().From(() => self.ID?.ToString() ?? "None", null);
        }
    }
}
