using System;
using System.Linq;
using Editor;
using MANIFOLD.AnimGraph.Nodes;
using Sandbox;

namespace MANIFOLD.AnimGraph.GraphTools {
    public class StateItem : GraphicsItem, IContextMenuSource, IDeletable {
        public const float RADIUS = 64f;
        public const string BASE_COLOR = "#3e5ec9";
        public const string START_COLOR = "#42c93e";
        public const string END_COLOR = "#c94e3e";
        public const string ALWAYS_EVAL_COLOR = "#b73ec9";

        public readonly StateMachineView view;
        public readonly StateMachine.State state;

        private bool rightMousePressed;
        private bool hasMoved;

        public Action PositionChanged { get; set; }

        public override Rect BoundingRect => base.BoundingRect.Grow(16f);

        public StateItem(StateMachineView view, StateMachine.State state) {
            this.view = view;
            this.state = state;

            Size = new Vector2(RADIUS * 2f, RADIUS * 2f);
            Position = state.Position;

            Movable = true;
            Selectable = true;
            HoverEvents = true;

            Cursor = CursorShape.Finger;
        }

        public override bool Contains(Vector2 localPos) {
            return (LocalRect.Center - localPos).LengthSquared < RADIUS * RADIUS;
        }

        protected override void OnMoved() {
            hasMoved = true;
            state.Position = Position.SnapToGrid(StateMachineView.GRID_SIZE);
            UpdatePosition();
        }

        protected override void OnMousePressed(GraphicsMouseEvent e) {
            base.OnMousePressed(e);

            if (e.RightMouseButton) {
                rightMousePressed = true;
                e.Accepted = true;
            }
        }

        protected override void OnMouseReleased(GraphicsMouseEvent e) {
            base.OnMouseReleased(e);
            
            if (e.RightMouseButton && rightMousePressed) {
                rightMousePressed = false;
                e.Accepted = true;
            }
            if (e.LeftMouseButton && !hasMoved) {
                view.OnStateSelected?.Invoke(state);
            }
            hasMoved = false;
        }

        protected override void OnMouseMove(GraphicsMouseEvent e) {
            if (rightMousePressed && !Contains(e.LocalPosition)) {
                rightMousePressed = false;
                view.StartCreatingTransition(this);
            }
            
            base.OnMouseMove(e);
        }

        public void OnContextMenu(ContextMenuEvent e) {
            var menu = new ContextMenu();
            menu.AddOption("Delete", "delete", Delete);
            menu.OpenAtCursor(true);

            e.Accepted = true;
        }

        protected override void OnPaint() {
            Color borderColor = Selected
                ? Color.Yellow
                : Hovered
                    ? Color.White
                    : Color.White.Darken(0.125f);

            Color fillColor = BASE_COLOR;
            if (state.Start) fillColor = START_COLOR;
            if (state.End) fillColor = END_COLOR;
            
            fillColor = fillColor
                .Lighten(Selected ? 0.5f : Hovered ? 0.25f : 0f)
                .Desaturate(Selected ? 0.5f : Hovered ? 0.25f : 0f);

            Paint.ClearPen();
            Paint.SetBrush(fillColor);
            Paint.DrawCircle(Size * 0.5f, Size);

            if (state.AlwaysEvaluate) {
                Paint.SetPen(ALWAYS_EVAL_COLOR, size: 12f);
                Paint.ClearBrush();
                Paint.DrawCircle(Size * 0.5f, Size - 12f);
            }
            
            Paint.SetPen(borderColor, Selected || Hovered ? 5f : 4f, state.PassThrough ? PenStyle.Dash : PenStyle.Solid);
            Paint.ClearBrush();
            Paint.DrawCircle(Size * 0.5f, Size);

            var titleRect = new Rect(0f, Size.y * 0.5f - 12f, Size.x, 24f);

            Paint.SetFont("roboto", 12f, 600);
            Paint.SetPen(Color.Black.WithAlpha(0.5f));
            Paint.DrawText(new Rect(titleRect.Position + 2f, titleRect.Size), state.Name);

            Paint.SetPen(borderColor);
            Paint.DrawText(titleRect, state.Name);
        }

        public void Delete() {
            var transitions = view.Items
                .OfType<TransitionItem>()
                .Where(x => x.From == this || x.To == this)
                .ToArray();

            foreach (var transition in transitions) {
                transition.Delete();
            }
            
            
            view.stateMachine.States.Remove(state.ID);
            view.stateMachine.ResizeArray();
                
            view.OnStateRemoved?.Invoke(state);
            Destroy();
        }
        
        public void UpdatePosition() {
            Position = state.Position;
            PositionChanged?.Invoke();
        }
    }
}
