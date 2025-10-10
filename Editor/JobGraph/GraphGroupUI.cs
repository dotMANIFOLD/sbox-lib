using System.Collections.Generic;
using System.Linq;
using Editor;
using Editor.NodeEditor;
using Sandbox;

namespace MANIFOLD.JobGraph.Editor {
    public class GraphGroupUI : NodeUI {
        public const float MARGIN = 48f;
        
        private readonly GraphGroupNode node;
        
        public GraphGroupUI(GraphView graph, GraphGroupNode node) : base(graph, node) {
            this.node = node;
            
            Selectable = false;
            Movable = false;
            HoverEvents = false;
            Cursor = CursorShape.None;
        }

        protected override void Layout() {
            if (node == null) return;
            if (node.Containing == null) return;
            if (!node.Containing.Any()) return;
            
            Rect rect = node.Containing.First().SceneRect;
            if (node.Containing.Count() > 1) {
                foreach (var elem in node.Containing.Skip(1)) {
                    rect.Add(elem.SceneRect);
                }
            }
                
            rect = rect.Grow(MARGIN);
            SceneRect = rect;
        }
        
        protected override void OnPaint() {
            var rect = LocalRect.Shrink(1);
            var color = node.GroupColor;
            Paint.SetPen(color);
            Paint.SetBrush(color.WithAlpha(0.1f));
            Paint.DrawRect(rect, 10);

            var textRect = rect.Shrink(10);
            Paint.SetHeadingFont(16, 600);
            Paint.DrawText(textRect, DisplayInfo.Name, TextFlag.LeftTop);
        }
    }
}
