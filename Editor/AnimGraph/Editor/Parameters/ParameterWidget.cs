using System;
using System.Reflection;
using Editor;
using Sandbox;

namespace MANIFOLD.AnimGraph.Editor {
    public class ParameterWidget : Widget {
        public const float COLOR_WIDTH = 8;
        public const float PADDING = 8;
        public const float TYPE_WIDTH = 60;
        
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

        public bool Selected { get; set; }
        
        public Action<ParameterWidget> OnSelected { get; set; }
        public Action<ParameterWidget> OnDuplicate { get; set; }
        public Action<ParameterWidget> OnDelete { get; set; }
        
        public ParameterWidget(Widget parent = null) : base(parent) {
            Layout = Layout.Row();
            FixedHeight = 24;
            SetSizeMode(SizeMode.Flexible, SizeMode.CanGrow);
        }

        protected override void OnMouseClick(MouseEvent e) {
            Selected = true;
            OnSelected?.Invoke(this);
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
            Rect rect = new Rect(0, Size);

            if (Selected) {
                Paint.SetBrushAndPen(Theme.SelectedBackground);
                Paint.DrawRect(rect);
            }
            
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
                nameRect = rect.Shrink(TYPE_WIDTH + PADDING + 2 + PADDING, 0, 0, 0);
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
