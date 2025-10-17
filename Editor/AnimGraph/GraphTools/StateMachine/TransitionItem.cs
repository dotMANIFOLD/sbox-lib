using System;
using Editor;
using MANIFOLD.AnimGraph.Nodes;
using Sandbox;

namespace MANIFOLD.AnimGraph.GraphTools {
    public class TransitionItem : GraphicsItem, IContextMenuSource, IDeletable {
        private StateMachine.Transition transition;
        private StateItem from;
        private bool isPreview;
        
        public StateMachine.Transition Transition => transition;
        public StateItem From => from;
        public StateItem To { get; set; }
        public Vector2 TargetPosition { get; set; }
        public bool IsPreview => isPreview;

        public override Rect BoundingRect => base.BoundingRect.Grow(12f);

        public TransitionItem(StateMachine.Transition transition, StateItem from, StateItem to) : base(null) {
            this.transition = transition;
            this.from = from;
            this.To = to;

            ZIndex = -10;

            if (to != null) {
                from.PositionChanged += OnStatePositionChanged;
                to.PositionChanged += OnStatePositionChanged;

                Cursor = CursorShape.Finger;

                Selectable = true;
                HoverEvents = true;
            } else {
                isPreview = true;
            }
        }

        protected override void OnDestroy() {
            if (isPreview) return;
            
            from.PositionChanged -= OnStatePositionChanged;
            To.PositionChanged -= OnStatePositionChanged;
        }

        protected override void OnMouseReleased(GraphicsMouseEvent e) {
            base.OnMouseReleased(e);

            if (e.LeftMouseButton) {
                from.view.OnTransitionSelected?.Invoke(transition);
                e.Accepted = true;
            }
        }

        public void OnContextMenu(ContextMenuEvent e) {
            if (IsPreview) return;

            e.Accepted = true;
            Selected = true;

            var menu = new ContextMenu();
            menu.AddOption("Delete", "delete", Delete);
            menu.OpenAtCursor(true);
        }

        protected override void OnPaint() {
            var start = new Vector2(0f, Size.y * 0.5f);
            var end = start.WithX(Size.x);
            var tangent = new Vector2(1f, 0f);

            var normal = tangent.Perpendicular;

            var selected = Selected || IsPreview;
            var thickness = selected || Hovered ? 6f : 4f;

            var offset = thickness * 0.5f * normal;
            Color color = Color.White;
            if (Hovered) color = Theme.Blue;
            if (selected) color = Color.Yellow;
            if (Transition != null && Transition.Disable) color = color.Darken(0.4f);

            var arrowEnd = end;
            var lineEnd = arrowEnd - tangent * 14f;
            
            Paint.ClearPen();
            Paint.SetBrush(color);
            Paint.DrawPolygon(start - offset, lineEnd - offset, lineEnd + offset, start + offset);
            var arrowScale = Hovered || selected ? 1.25f : 1f;
            Paint.DrawArrow(arrowEnd - tangent * 16f * arrowScale, arrowEnd, 12f * arrowScale);

            if (transition != null) {
                float conditionRadius = 24;
                float conditionSpacing = 8;
                float spaceUsed = (conditionRadius * transition.Conditions.Count) + (conditionSpacing * Math.Max(0, transition.Conditions.Count - 1));
                Vector2 position = start.LerpTo(end, 0.5f);
                position.x -= spaceUsed * 0.5f;
                foreach (var condition in transition.Conditions) {
                    Paint.Antialiasing = true;
                
                    Paint.SetPen(Color.White, size: conditionRadius * 0.14f);
                    Paint.SetBrush(condition.Color);
                    Paint.DrawCircle(position, conditionRadius);
                
                    position.x += conditionRadius + conditionSpacing;
                }
            }
        }

        public void Layout() {
            PrepareGeometryChange();

            var lineData = GetSceneStartEnd();
            if (lineData == null) {
                Size = 0f;
            } else {
                var diff = lineData.Value.end - lineData.Value.start;
                var length = diff.Length;

                Position = lineData.Value.start - lineData.Value.tangent.Perpendicular * 8f;
                Size = new Vector2(length, 16f);
                Rotation = MathF.Atan2(diff.y, diff.x) * 180 / MathF.PI;
            }
            
            if (To != null) {
                ToolTip = $"Transition <b>{from.state.Name}</b> \u2192 <b>{To.state.Name}</b>";
            }
            
            Update();
        }

        public void Delete() {
            from.view.stateMachine.Transitions.Remove(Transition);
            Destroy();
            from.view.UpdateTransitionNeighbors();
        }
        
        private (Vector2 start, Vector2 end, Vector2 tangent)? GetSceneStartEnd() {
            var (index, count) = from.view.GetTransitionPosition(this);

            var fromCenter = from.Center;
            var toCenter = To?.Center ?? TargetPosition;
            if ((toCenter - fromCenter).IsNearZeroLength) return null;
            
            var tangent = (toCenter - fromCenter).Normal;
            var normal = tangent.Perpendicular;

            if (To == null || To.state.ID.CompareTo(From.state.ID) > 0) {
                normal = -normal;
            }

            var maxWidth = StateItem.RADIUS * 2f;
            var usedWidth = count * 48f;
            
            var itemWidth = Math.Min( usedWidth, maxWidth ) / count;
            var offset = (index - count * 0.5f + 0.5f) * itemWidth;
            
            var curve = MathF.Sqrt(StateItem.RADIUS * StateItem.RADIUS - offset * offset);

            var start = fromCenter + tangent * curve;
            var end = toCenter - tangent * (To == null ? 0f : curve);
            
            return (start + offset * normal, end + offset * normal, tangent);
        }
        
        private void OnStatePositionChanged() {
            Layout();
        }
    }
}
