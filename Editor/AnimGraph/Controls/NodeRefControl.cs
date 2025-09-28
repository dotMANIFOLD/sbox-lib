using System;
using Editor;
using Sandbox;

namespace MANIFOLD.AnimGraph.Editor {
    [CustomEditor(typeof(NodeRef))]
    public class NodeRefControl : ControlWidget {
        private readonly SerializedObject obj;
        private readonly SerializedProperty idProperty;
        
        public NodeRefControl(SerializedProperty property) : base(property) {
            Layout = Layout.Column();
            Layout.Margin = 4;

            property.TryGetAsObject(out obj);
            if (obj == null) {
                Log.Warning("Failed to convert NodeRef to serialized object");
                return;
            }
            idProperty = obj.GetProperty("ID");
            
            Rebuild();
        }

        protected override void PaintUnder() {
            
        }

        [Event(InspectorPanel.EVENT_REBUILD)]
        private void Rebuild() {
            Layout.Clear(true);

            var label = Layout.Add(new Label());
            var graph = GetContext<AnimGraph>(AnimGraphEditor.CONTEXT_GRAPH);
            
            if (graph != null) {
                label.Bind("Text").ReadOnly().From(() => {
                    var id = idProperty.GetValue<Guid?>();
                    if (id.HasValue) {
                        var node = graph.Nodes[id.Value];
                        return node.DisplayName;
                    } else {
                        return "None";
                    }
                }, null);
            } else {
                label.Bind("Text").ReadOnly().From(() => idProperty.GetValue<Guid?>()?.ToString() ?? "None", null);
            }
        }
    }
}
