using Editor;
using Sandbox;

namespace MANIFOLD.AnimGraph.Editor {
    public class TagWidget : Widget {
        public const float COLOR_WIDTH = 8;
        public const float PADDING = 8;
        public const float CIRCLE_PADDING = 4;
        public const float CIRCLE_INNER_PADDING = 3;
        public const float TEXT_SPACING = 8;
        
        private readonly AnimGraphEditor editor;

        private Tag tag;
        private Color rectColor;
        private Rect nameRect;

        public Tag Tag {
            get => tag;
            set {
                tag = value;
                GetRenderValues();
            }
        }

        public Tag PreviewTag { get; set; }
        
        public TagWidget(AnimGraphEditor editor, Widget parent = null) : base(parent) {
            this.editor = editor;

            FixedHeight = 24;
            FocusMode = FocusMode.Click;

            Layout = Layout.Row();
        }

        protected override void OnDoubleClick(MouseEvent e) {
            if (nameRect.IsInside(e.LocalPosition)) {
                Layout.Clear(true);
                
                LineEdit edit = new LineEdit(this);
                edit.DeleteOnClose = true;
                edit.Text = tag.Name;
                edit.Focus();
                edit.SelectAll();
                edit.EditingFinished += () => {
                    tag.Name = edit.Text.Trim();
                    Layout.Clear(true);
                    Update();
                };

                Layout.AddSpacingCell(nameRect.Left);
                Layout.Add(edit);
                Layout.AddSpacingCell(PADDING);
            }
        }

        protected override void OnContextMenu(ContextMenuEvent e) {
            ContextMenu menu = new ContextMenu();
            menu.AddOption("Delete", "delete", DeleteTag);
            menu.OpenAt(e.ScreenPosition);
        }

        protected override void OnPaint() {
            Rect rect = LocalRect.Shrink(0, 0, 1, 1);
            
            // BACKGROUND
            Paint.SetBrush(IsFocused ? Theme.SelectedBackground : Theme.BaseAlt);
            Paint.SetPen(Theme.SurfaceLightBackground);
            Paint.DrawRect(rect);

            rect = rect.Shrink(1, 1, 0, 0);
            
            // COLOR RECT
            Rect colorRect = rect;
            colorRect.Width = COLOR_WIDTH;
            Paint.SetBrush(rectColor);
            Paint.ClearPen();
            Paint.DrawRect(colorRect);

            rect = rect.Shrink(COLOR_WIDTH, 0, 0, 0).Shrink(PADDING, 0);

            if (tag == null) {
                Paint.SetPen(Theme.Text);
                Paint.ClearBrush();

                Paint.DrawText(rect, "Null Tag", TextFlag.LeftCenter);
            } else {
                Rect circleAreaRect = rect;
                circleAreaRect.Width = rect.Height;

                Paint.Antialiasing = true;
                
                Rect circleRect = circleAreaRect.Shrink(CIRCLE_PADDING);
                Paint.SetPen(Color.White.Darken(0.2f), 2);
                Paint.ClearBrush();
                Paint.DrawCircle(circleRect);

                if (PreviewTag != null) {
                    circleRect = circleRect.Shrink(CIRCLE_INNER_PADDING);
                    Color innerCircleColor = tag.State ? Theme.Green : Theme.Red;
                    Paint.SetBrushAndPen(innerCircleColor);
                    Paint.DrawCircle(circleRect);
                } 

                rect.Left = circleAreaRect.Right;
                nameRect = rect.Shrink(TEXT_SPACING, 0, 0, 0);
                var nameSize = Paint.MeasureText(tag.Name);
                nameRect.Width = nameSize.x;
                Paint.SetPen(Theme.Text);
                Paint.ClearBrush();
                Paint.DrawText(nameRect, tag.Name, TextFlag.LeftCenter);
            }
        }

        public void DeleteTag() {
            editor.GraphResource.Tags.Remove(tag.ID);
            editor.GraphResource.StateHasChanged();
            EditorEvent.Run(TagPanel.EVENT_REFRESH);
        }
        
        private void GetRenderValues() {
            rectColor = tag.Type switch {
                Tag.TagType.Event => "#2250AB",
                Tag.TagType.Internal => "#AB2252",
                _ => "#FF00FF"
            };
        }
    }
}
