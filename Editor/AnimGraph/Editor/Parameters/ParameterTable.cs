using Editor;
using MANIFOLD.AnimGraph.Parameters;
using Sandbox;

namespace MANIFOLD.AnimGraph.Editor {
    public class ParameterTable : Widget {
        private readonly AnimGraphEditor editor;
        private readonly GridLayout gridLayout;
        
        public ParameterTable(AnimGraphEditor editor) : base(null) {
            this.editor = editor;

            gridLayout = Layout.Grid();
            gridLayout.HorizontalSpacing = 2;
            gridLayout.SizeConstraint = SizeConstraint.SetMaximumSize;
            gridLayout.SetColumnStretch(1, 0);
            gridLayout.SetMinimumColumnWidth(0, 140);
            
            Layout = gridLayout;
        }

        [Event(AnimGraphEditor.EVENT_PREVIEW)]
        [Event(AnimGraphEditor.EVENT_GRAPH_LOAD)]
        public void Rebuild() {
            gridLayout.Clear(true);

            gridLayout.SetColumnStretch(4, editor.InPreview ? 1 : 0);
            
            int row = 0;
            foreach (var param in editor.GraphResource.Parameters.Values) {
                var widget = new ParameterWidget(editor);
                widget.Parameter = param;
                gridLayout.AddCell(0, row, widget);
                
                if (editor.InPreview) {
                    var serialized = editor.PreviewAnimator.Parameters.Get(param.ID).GetSerialized();
                    var prop = serialized.GetProperty("Value");
                    var control = CreateControlWidget(param, prop);
                    control.HorizontalSizeMode = SizeMode.Default;
                    gridLayout.AddCell(1, row, control);
                }
                row++;
            }
        }

        private ControlWidget CreateControlWidget(Parameter param, SerializedProperty valueProp) {
            if (param is FloatParameter floatParam) {
                var floatControl = new FloatControlWidget(valueProp);
                if (floatParam.HasRange) {
                    var range = new Vector2(floatParam.MinValue, floatParam.MaxValue);
                    floatControl.MakeRanged(range, 0, false, true);
                }
                return floatControl;
            }

            return ControlWidget.Create(valueProp);
        }
    }
}
