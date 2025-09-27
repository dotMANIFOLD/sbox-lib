using System;
using System.Linq;
using Editor;
using Sandbox;

namespace MANIFOLD.AnimGraph.Editor {
    [CustomEditor(typeof(ParameterRef<>))]
    public class ParamRefControl : ControlWidget {
        public ParamRefControl(SerializedProperty property) : base(property) {
            Layout = Layout.Column();
            Layout.Spacing = 4;

            bool success = property.TryGetAsObject(out SerializedObject serializedObj);
            if (!success) {
                throw new Exception("Failed to convert parameter ref to object");
            }
            
            Rebuild();
        }
        
        [Event(InspectorPanel.EVENT_REBUILD)]
        private void Rebuild() {
            Layout.Clear(true);
            
            var currentId = (Guid?)SerializedProperty.PropertyType.GetProperty("ID").GetValue(SerializedProperty.GetValue<object>());
            
            var type = SerializedProperty.PropertyType;
            var parameterType = type.GenericTypeArguments[0];
            var scanType = typeof(Parameter<>).MakeGenericType(parameterType);
            
            var graph = GetContext<AnimGraph>(AnimGraphEditor.CONTEXT_GRAPH);
            if (graph != null) {
                var validParameters = graph.Parameters.Values.Where(x => x.GetType().IsAssignableTo(scanType));
                
                var comboBox = Layout.Add(new ComboBox(this));
                comboBox.AddItem("<None>", onSelected: () => SerializedProperty.SetValue(CreateNewRef(null)), selected: currentId == null);
                foreach (var param in validParameters) {
                    comboBox.AddItem(param.Name, onSelected: () => SerializedProperty.SetValue(CreateNewRef(param.ID)), selected: currentId == param.ID);   
                }
            } else {
                Layout.Add(new Label(currentId.HasValue ? currentId.Value.ToString() : "None"));
            }
        }

        private object CreateNewRef(Guid? id) {
            var newRef = Activator.CreateInstance(SerializedProperty.PropertyType);
            var prop = newRef.GetType().GetProperty("ID");
            prop.SetValue(newRef, id);
            return newRef;
        }
    }
}
