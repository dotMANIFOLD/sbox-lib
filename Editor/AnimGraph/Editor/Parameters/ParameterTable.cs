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
            gridLayout.SetMinimumColumnWidth(0, 200);
            
            Layout = gridLayout;
        }

        [Event(AnimGraphEditor.EVENT_PREVIEW)]
        [Event(AnimGraphEditor.EVENT_GRAPH_LOAD)]
        [Event(ParameterPanel.EVENT_REFRESH)]
        public void Rebuild() {
            gridLayout.Clear(true);

            gridLayout.SetColumnStretch(1, editor.InPreview ? 1 : 0);
            
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
            } else if (param is IntParameter intParam) {
                var intControl = new IntegerControlWidget(valueProp);
                if (intParam.HasRange) {
                    var range = new Vector2(intParam.MinValue, intParam.MaxValue);
                    intControl.MakeRanged(range, 1, false, true);
                }
                return intControl;
            }

            return ControlWidget.Create(valueProp);
        }
    }
}
