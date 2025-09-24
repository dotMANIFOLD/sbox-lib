using System;
using System.Reflection;
using Editor;
using Sandbox;

namespace MANIFOLD.AnimGraph.Editor {
    public class ParameterWidget : Widget {
        public const float COLOR_WIDTH = 8;
        public const float PADDING = 8;
        public const float TYPE_WIDTH = 60;

        private readonly AnimGraphEditor editor;
        
        private Parameter param;
        
        private string typeString;
        private Color rectColor;
        private Rect nameRect;

        public Parameter Parameter {
            get => param;
            set {
                param = value;
                GetRenderValues();
            }
        }
        
        public Action<ParameterWidget> OnDuplicate { get; set; }
        public Action<ParameterWidget> OnDelete { get; set; }
        
        public ParameterWidget(AnimGraphEditor editor, Widget parent = null) : base(parent) {
            this.editor = editor;
            
            FixedHeight = 24;
            FocusMode = FocusMode.Click;
            
            Layout = Layout.Row();
        }
        
        protected override void OnFocus(FocusChangeReason reason) {
            base.OnFocus(reason);
            editor.SelectedParameter = Parameter;
        }

        protected override void OnDoubleClick(MouseEvent e) {
            if (nameRect.IsInside(e.LocalPosition)) {
                Layout.Clear(true);
                
                LineEdit edit = new LineEdit(this);
                edit.DeleteOnClose = true;
                edit.Text = param.Name;
                edit.Focus();
                edit.SelectAll();
                edit.EditingFinished += () => {
                    param.Name = edit.Text.Trim();
                    Layout.Clear(true);
                    Update();
                };

                Layout.AddSpacingCell(nameRect.Left);
                Layout.Add(edit);
                Layout.AddSpacingCell(PADDING);
            }
        }

        protected override void OnContextMenu(ContextMenuEvent e) {
            ContextMenu menu = new ContextMenu(this);
            menu.AddOption("Duplicate", "content_copy", () => OnDuplicate?.Invoke(this));
            menu.AddOption("Delete", "delete", () => OnDelete?.Invoke(this));
            menu.OpenAt(e.ScreenPosition);
        }

        protected override void OnPaint() {
            Rect rect = LocalRect.Shrink(0, 0, 1, 1);
            
            Paint.SetBrush(IsFocused ? Theme.SelectedBackground : Theme.BaseAlt);
            Paint.SetPen(Theme.SurfaceLightBackground);
            Paint.DrawRect(rect);

            rect = rect.Shrink(1, 1, 0, 0);

            Rect colorRect = rect;
            colorRect.Width = COLOR_WIDTH;
            Paint.SetBrush(rectColor);
            Paint.ClearPen();
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
                nameRect = rect.Shrink(TYPE_WIDTH + PADDING + 2 + PADDING, 0, 0, 0);
                var nameSize = Paint.MeasureText(param.Name);
                nameRect.Width = MathF.Max(nameSize.x, 120);
                Paint.SetPen(Theme.Text);
                Paint.DrawText(nameRect, param.Name, TextFlag.LeftCenter);
            }
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
