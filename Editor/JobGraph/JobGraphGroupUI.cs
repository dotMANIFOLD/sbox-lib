using System.Collections.Generic;
using System.Linq;
using Editor;
using Editor.NodeEditor;
using Sandbox;

namespace MANIFOLD.JobGraph.Editor {
    public class JobGraphGroupUI : NodeUI {
        public const float MARGIN = 48f;
        
        private readonly JobGraphNode node;
        private readonly IEnumerable<NodeUI> contains;
        
        public JobGraphGroupUI(GraphView graph, JobGraphNode node, IEnumerable<NodeUI> contains) : base(graph, node) {
            this.node = node;
            this.contains = contains;
            Selectable = false;
            Movable = false;
            HoverEvents = false;
            Cursor = CursorShape.None;
        }

        protected override void Layout() {
            if (contains == null) return;
            
            Rect rect = contains.First().SceneRect;
            foreach (var elem in contains.Skip(1)) {
                rect.Add(elem.SceneRect);
            }
            rect = rect.Grow(MARGIN);
            SceneRect = rect;
        }
        
        protected override void OnPaint() {
            var rect = LocalRect.Shrink(1);
            var color = node.Wrapper.GroupColors[node.Job.ID];
            Paint.SetPen(color);
            Paint.SetBrush(color.WithAlpha(0.1f));
            Paint.DrawRect(rect, 10);

            var textRect = rect.Shrink(10);
            Paint.SetHeadingFont(16, 600);
            Paint.DrawText(textRect, DisplayInfo.Name, TextFlag.LeftTop);
        }
    }
}
