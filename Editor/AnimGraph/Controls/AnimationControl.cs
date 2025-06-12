using System.Linq;
using Editor;
using Sandbox;

namespace MANIFOLD.AnimGraph.Editor {
    [CustomEditor(typeof(string), WithAllAttributes = [ typeof(AnimationAttribute) ])]
    public class AnimationControl : ControlWidget {
        public AnimationControl(SerializedProperty property) : base(property) {
            Layout = Layout.Row();

            var node = (JobNode)property.Parent.Targets.First();
            var currentValue = property.GetValue<string>();
            
            var comboBox = new ComboBox(this);
            foreach (var anim in node.Graph.Collection.Animations) {
                comboBox.AddItem(anim.Name, onSelected: () => property.SetValue(anim.Name), selected: anim.Name == currentValue);
            }
            
            Layout.Add(comboBox);
        }
    }
}
