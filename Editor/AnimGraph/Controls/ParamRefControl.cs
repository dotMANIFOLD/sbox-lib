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
            
            var node = property.Parent.Targets.First() as JobNode;
            if (node == null) {
                Layout.Add(new Label("Parent Object is not a node!", this));
                return;
            }
            
            var type = property.PropertyType;
            var self = property.GetValue<ParameterRef>();

            var parameterType = type.GenericTypeArguments[0];
            
            var scanType = typeof(Parameter<>).MakeGenericType(parameterType);
            var validParameters = node.Graph.Parameters.Values.Where(x => x.GetType().IsAssignableTo(scanType));
            
            var comboBox = new ComboBox(this);
            comboBox.AddItem("<None>", onSelected: () => self.ID = null, selected: self.ID == null);
            foreach (var param in validParameters) {
                comboBox.AddItem(param.Name, onSelected: () => self.ID = param.ID, selected: self.ID == param.ID);
            }
            
            Layout.Add(comboBox);
        }
    }
}
