using System;
using System.Collections.Generic;
using System.Linq;
using Editor;
using MANIFOLD.AnimGraph.Editor;
using MANIFOLD.AnimGraph.Nodes;
using Sandbox;

namespace MANIFOLD.AnimGraph.GraphTools {
    public class StateMachineView : GraphicsView {
        public const float GRID_SIZE = 32f;
        
        public readonly StateMachine stateMachine;

        private Dictionary<StateMachine.State, StateItem> states;
        private Dictionary<StateMachine.Transition, TransitionItem> transitions;
        private Dictionary<UnorderedPair<Guid>, List<TransitionItem>> transitionNeighbors;
        private TransitionItem previewTransition;
        
        private Vector2 lastMouseScenePosition;
        private bool wasDraggingTransition;
        
        public Action<StateMachine.State> OnStateAdded { get; set; }
        public Action<StateMachine.State> OnStateRemoved { get; set; }
        public Action<StateMachine.State> OnStateSelected { get; set; }
        public Action<StateMachine.Transition> OnTransitionSelected { get; set; }
        
        public StateMachineView(StateMachine stateMachine) {
            this.stateMachine = stateMachine;
            
            SetBackgroundImage( "toolimages:/grapheditor/grapheditorbackgroundpattern_shader.png" );

            Antialiasing = true;
            TextAntialiasing = true;
            BilinearFiltering = true;

            SceneRect = new Rect( -100000, -100000, 200000, 200000 );

            HorizontalScrollbar = ScrollbarMode.Off;
            VerticalScrollbar = ScrollbarMode.Off;
            MouseTracking = true;
            
            states = new Dictionary<StateMachine.State, StateItem>();
            transitions = new Dictionary<StateMachine.Transition, TransitionItem>();
            transitionNeighbors = new Dictionary<UnorderedPair<Guid>, List<TransitionItem>>();
            
            RebuildItems();
        }

        protected override void OnWheel(WheelEvent e) {
            Zoom(e.Delta > 0 ? 1.1f : 0.90f, e.Position);
            e.Accepted = true;
        }

        protected override void OnMousePress(MouseEvent e) {
            base.OnMousePress(e);

            if (e.MiddleMouseButton) {
                e.Accepted = true;
                return;
            }
            if (e.RightMouseButton) {
                e.Accepted = GetItemAt(ToScene(e.LocalPosition)) == null;
                return;
            }
        }

        protected override void OnMouseReleased(MouseEvent e) {
            base.OnMouseReleased(e);
            
            if (previewTransition.IsValid()) {
                if (previewTransition.To != null) {
                    var transition = new StateMachine.Transition() {
                        From = previewTransition.From.state.ID,
                        To = previewTransition.To.state.ID,
                    };
                    stateMachine.Transitions.Add(transition);
                    
                    AddTransitionItem(transition);
                }
                
                previewTransition?.Destroy();
                previewTransition = null;

                wasDraggingTransition = true;
                e.Accepted = true;
                
                UpdateTransitionNeighbors();
            }
        }

        protected override void OnMouseMove(MouseEvent e) {
            var scenePos = ToScene(e.LocalPosition);
            
            if (e.ButtonState.HasFlag(MouseButtons.Middle)) {
                var delta = scenePos - lastMouseScenePosition;
                Translate(delta);
                e.Accepted = true;
                Cursor = CursorShape.ClosedHand;
            } else {
                Cursor = CursorShape.None;
            }

            if (previewTransition.IsValid()) {
                var oldTarget = previewTransition.To;

                previewTransition.TargetPosition = scenePos;

                if (GetItemAt(scenePos) is StateItem newTarget && newTarget != previewTransition.From) {
                    previewTransition.To = newTarget;
                } else {
                    previewTransition.To = null;
                }

                if (oldTarget != previewTransition.To) UpdateTransitionNeighbors();
                previewTransition.Layout();
            }
            
            lastMouseScenePosition = ToScene(e.LocalPosition);
        }

        protected override void OnContextMenu(ContextMenuEvent e) {
            if (wasDraggingTransition) return;
            
            var menu = new ContextMenu();
            var scenePos = ToScene( e.LocalPosition );

            if (GetItemAt(scenePos) is IContextMenuSource source) {
                source.OnContextMenu(e);
                if (e.Accepted) return;
            }

            e.Accepted = true;
            
            menu.AddOption("Create new state", action: () => {
                var state = new StateMachine.State();
                state.Position = scenePos.SnapToGrid(GRID_SIZE) - StateItem.RADIUS;
                stateMachine.States.Add(state.ID, state);
                stateMachine.ResizeArray();
                AddStateItem(state);
                OnStateAdded?.Invoke(state);
            });
            
            menu.OpenAtCursor(true);
        }

        [EditorEvent.Frame]
        private void OnFrame() {
            wasDraggingTransition = false;
        }
        
        public void RebuildItems() {
            states.Clear();
            DeleteAllItems();

            foreach (var state in stateMachine.States.Values) {
                AddStateItem(state);
            }
            foreach (var transition in stateMachine.Transitions) {
                AddTransitionItem(transition);
            }
            
            UpdateTransitionNeighbors();
        }

        public void StartCreatingTransition(StateItem from, StateMachine.Transition copy = null) {
            previewTransition?.Destroy();

            previewTransition = new TransitionItem(copy, from, null) {
                TargetPosition = from.Center
            };
            previewTransition.Layout();

            Add(previewTransition);
        }
        
        public (int index, int count) GetTransitionPosition(TransitionItem item) {
            if (item.To == null) return (0, 1);
            
            var key = new UnorderedPair<Guid>(item.From.state.ID, item.To.state.ID);
            if (!transitionNeighbors.TryGetValue(key, out var list)) {
                return (0, 1);
            }
            return (list.IndexOf(item), list.Count);
        }

        public void UpdateTransitionNeighbors() {
            transitionNeighbors.Clear();

            foreach (var item in Items.OfType<TransitionItem>().Where(x => x.To != null)) {
                var key = new UnorderedPair<Guid>(item.From.state.ID, item.To.state.ID);
                if (!transitionNeighbors.TryGetValue(key, out var list)) {
                    transitionNeighbors[key] = list = new List<TransitionItem>();
                }
                
                list.Add(item);
            }

            foreach (var list in transitionNeighbors.Values) {
                foreach (var item in list) {
                    item.Layout();
                }
            }
        }
        
        private void AddStateItem(StateMachine.State state) {
            var item = new StateItem(this, state);
            states.Add(state, item);
            Add(item);
        }

        private void AddTransitionItem(StateMachine.Transition transition) {
            var from = states.GetValueOrDefault(stateMachine.States[transition.From]);
            var to = states.GetValueOrDefault(stateMachine.States[transition.To]);

            if (from == null || to == null) return;

            var item = new TransitionItem(transition, from, to);
            transitions.Add(transition, item);
            Add(item);
        }
    }
}
