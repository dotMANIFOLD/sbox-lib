using System.Linq;
using Editor;
using Sandbox;

namespace MANIFOLD.AnimGraph.Editor {
    [CustomEditor(typeof(MANIFOLDAnimator))]
    public class AnimatorInspector : ComponentEditorWidget {
        private ControlSheet normalSheet;
        
        private Group parameterGroup;
        private Widget parameterGroupCanvas;
        private ControlSheet parameterSheet;

        private ParameterList lastList;

        public MANIFOLDAnimator Target => SerializedObject.Targets.First() as MANIFOLDAnimator;
        
        public AnimatorInspector(SerializedObject so) : base(so) {
            Layout = Layout.Column();
            Layout.Margin = 4;
            Layout.Spacing = 4;
            
            // create normal sheet
            normalSheet = new ControlSheet();
            Layout.Add(normalSheet);
            RebuildSheet();
            
            // create parameters group
            parameterGroupCanvas = new Widget(this);
            parameterGroupCanvas.Name = "Parameters";
            parameterGroupCanvas.Layout = Layout.Column();
            Layout.Add(parameterGroupCanvas);
            
            parameterSheet = new ControlSheet();
            Layout.Add(parameterSheet);
            RebuildParameters();
        }

        [EditorEvent.Frame]
        private void OnFrame() {
            if (Target == null) return;

            // parameterGroup.Visible = Target.IsPlaying;
            
            if (lastList != Target.Parameters) {
                RebuildParameters();
                lastList = Target.Parameters;
            }
        }
        
        [EditorEvent.Hotload]
        private void RebuildSheet() {
            normalSheet.Clear(true);
            normalSheet.AddObject(SerializedObject, (prop) => prop.HasAttribute<PropertyAttribute>() && !prop.Name.StartsWith("OnComponent"));
        }

        private void RebuildParameters() {
            parameterSheet.Clear(true);
            if (Target.Parameters == null) return;

            var parameters = Target.Parameters;
            // var properties = parameters.Select(x => x.Name).Zip(parameters.Select(x => x.GetSerialized().GetProperty("Value")));
            var results = parameters.Select(x => x.GetSerialized().GetProperty("Value")).ToList();
            Log.Info($"parameter count: {results.Count}");
            
            parameterSheet.AddPropertiesWithGrouping(results);
            Update(); 
        }
    }
}
