using System;
using System.Linq;
using Editor;
using Sandbox;

namespace MANIFOLD.AnimGraph.Editor {
    [CustomEditor(typeof(ParameterRef<>))]
    public class ParamRefControl : ControlWidget {
        private SerializedProperty idProperty;
        
        public ParamRefControl(SerializedProperty property) : base(property) {
            Layout = Layout.Column();
            Layout.Spacing = 4;

            bool success = property.TryGetAsObject(out SerializedObject serializedObj);
            if (!success) {
                throw new Exception("Failed to convert parameter ref to object");
            }

            idProperty = serializedObj.GetProperty("ID");
            
            Rebuild();
        }
        
        [Event(InspectorPanel.EVENT_REBUILD)]
        private void Rebuild() {
            Layout.Clear(true);

            if (idProperty == null) return;
            Guid? propValue = idProperty.GetValue<Guid?>();
            
            var type = SerializedProperty.PropertyType;
            var parameterType = type.GenericTypeArguments[0];
            var scanType = typeof(Parameter<>).MakeGenericType(parameterType);
            
            var graph = GetContext<AnimGraph>(AnimGraphEditor.CONTEXT_GRAPH);
            if (graph != null) {
                var validParameters = graph.Parameters.Values.Where(x => x.GetType().IsAssignableTo(scanType));
                
                var comboBox = Layout.Add(new ComboBox(this));
                comboBox.AddItem("<None>", onSelected: () => idProperty.SetValue((Guid?)null), selected: propValue == null);
                foreach (var param in validParameters) {
                    comboBox.AddItem(param.Name, onSelected: () => idProperty.SetValue(param.ID), selected: propValue == param.ID);   
                }
            } else {
                Layout.Add(new Label(propValue.HasValue ? propValue.Value.ToString() : "None"));
            }
        }
    }
}
