using System.Reflection;
using Editor;
using Sandbox;

namespace MANIFOLD.AnimGraph.Editor {
    public class ParameterWidget : Widget {
        public const float COLOR_WIDTH = 8;
        public const float PADDING = 8;
        public const float TYPE_WIDTH = 60;
        
        private Parameter param;
        private Color rectColor;
        private string typeString;

        public Parameter Parameter {
            get => param;
            set {
                param = value;
                GetRenderValues();
            }
        }
        
        public ParameterWidget(Widget parent = null) : base(parent) {
            FixedHeight = 24;
            SetSizeMode(SizeMode.Flexible, SizeMode.CanGrow);
        }

        protected override void OnPaint() {
            Rect rect = new Rect(0, Size);

            Rect colorRect = new Rect(0, 0, COLOR_WIDTH, Size.y);
            Paint.SetBrushAndPen(rectColor);
            Paint.DrawRect(colorRect);

            rect = rect.Shrink(COLOR_WIDTH, 0, 0, 0).Shrink(PADDING, 0);

            if (param == null) {
                Paint.SetPen(Theme.Text);
                Paint.ClearBrush();
            
                Paint.DrawText(rect, "Null Parameter", TextFlag.LeftCenter);
            } else {
                // TYPE
                Rect typeRect = rect;
                typeRect.Width = TYPE_WIDTH;
            
                Paint.SetPen(Theme.Text);
                Paint.ClearBrush();
                Paint.DrawText(typeRect, typeString);
            
                // SEPARATOR
                Rect separationRect = rect.Shrink(TYPE_WIDTH + PADDING, 0, 0, 0).Shrink(0, Size.y * 0.3f);
                separationRect.Width = 2;
                Paint.SetBrushAndPen(Theme.SurfaceLightBackground);
                Paint.DrawRect(separationRect);
            
                // NAME
                Rect nameRect = rect.Shrink(TYPE_WIDTH + PADDING + 2 + PADDING, 0, 0, 0);
                Paint.SetPen(Theme.Text);
                Paint.DrawText(nameRect, param.Name, TextFlag.LeftCenter);
            }
            
            // BORDER
            Paint.SetPen(Theme.SurfaceLightBackground);
            Paint.ClearBrush();
            Paint.DrawRect(new Rect(0, Size).Shrink(0, 0, 1, 1));
        }

        private void GetRenderValues() {
            var type = param.GetType();
            var exposeAttr = type.GetCustomAttribute<ExposeToAnimGraphAttribute>();
            var titleAttr = type.GetCustomAttribute<TitleAttribute>();
            var arg = type.BaseType.GenericTypeArguments[0];
            
            typeString = titleAttr?.Value ?? arg.Name;
            rectColor = exposeAttr?.Color ?? Color.White;
        }
    }
}
