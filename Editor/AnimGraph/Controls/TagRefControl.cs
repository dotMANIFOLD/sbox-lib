using System;
using System.Linq;
using Editor;
using Sandbox;

namespace MANIFOLD.AnimGraph.Editor {
    [CustomEditor(typeof(TagRef))]
    public class TagRefControl : ControlWidget {
        private SerializedProperty idProperty;
        
        public TagRefControl(SerializedProperty property) : base(property) {
            Layout = Layout.Column();
            Layout.Spacing = 4;

            bool success = property.TryGetAsObject(out SerializedObject serializedObj);
            if (!success) {
                throw new Exception("Failed to convert tag ref to object");
            }

            idProperty = serializedObj.GetProperty("ID");
            
            Rebuild();
        }
        
        [Event(InspectorPanel.EVENT_REBUILD)]
        private void Rebuild() {
            Layout.Clear(true);

            if (idProperty == null) return;
            Guid? propValue = idProperty.GetValue<Guid?>();
            
            var graph = GetContext<AnimGraph>(AnimGraphEditor.CONTEXT_GRAPH);
            if (graph != null) {
                var comboBox = Layout.Add(new ComboBox(this));
                comboBox.AddItem("<None>", onSelected: () => idProperty.SetValue((Guid?)null), selected: propValue == null);
                foreach (var tag in graph.Tags.Values) {
                    comboBox.AddItem(tag.Name, onSelected: () => idProperty.SetValue(tag.ID), selected: propValue == tag.ID);   
                }
            } else {
                Layout.Add(new Label(propValue.HasValue ? propValue.Value.ToString() : "None"));
            }
        }
    }
}
