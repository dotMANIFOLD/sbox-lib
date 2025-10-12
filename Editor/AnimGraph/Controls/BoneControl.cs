using System;
using Editor;
using Sandbox;

namespace MANIFOLD.AnimGraph.Editor {
    [CustomEditor(typeof(string), WithAllAttributes = [ typeof(BoneAttribute) ])]
    public class BoneControl : ControlWidget {
        public BoneControl(SerializedProperty property) : base(property) {
            Layout = Layout.Column();
            Layout.Spacing = 4;
            
            Rebuild();
        }

        [Event(InspectorPanel.EVENT_REBUILD)]
        private void Rebuild() {
            Layout.Clear(true);

            var currentBone = SerializedProperty.GetValue<string>();
            
            var graph = GetContext<AnimGraph>(AnimGraphEditor.CONTEXT_GRAPH);
            if (graph != null && graph.Resources != null) {
                var comboBox = Layout.Add(new ComboBox(this));
                comboBox.AddItem("<None>", onSelected: () => SerializedProperty.SetValue((string)null), selected: currentBone == null);
                foreach (var bone in graph.Resources.Model.Bones.AllBones) {
                    comboBox.AddItem(bone.Name, onSelected: () => SerializedProperty.SetValue(bone.Name), selected: currentBone == bone.Name);
                }
            } else {
                Layout.Add(new StringControlWidget(SerializedProperty));
            }
        }
    }
}
