using System.Linq;
using Editor;
using Sandbox;

namespace MANIFOLD.AnimGraph.Editor {
    [CustomEditor(typeof(string), WithAllAttributes = [ typeof(AnimationAttribute) ])]
    public class AnimationControl : ControlWidget {
        public AnimationControl(SerializedProperty property) : base(property) {
            Layout = Layout.Row();

            var node = property.Parent.Targets.First() as JobNode;
            if (node == null) {
                Layout.Add(new Label("Parent Object is not a node!", this));
                return;
            }
            
            var currentValue = property.GetValue<string>();
            
            var comboBox = new ComboBox(this);
            foreach (var anim in node.Graph.Collection.Animations) {
                comboBox.AddItem(anim.Name, onSelected: () => property.SetValue(anim.Name), selected: anim.Name == currentValue);
            }
            
            Layout.Add(comboBox);
        }
    }
}
