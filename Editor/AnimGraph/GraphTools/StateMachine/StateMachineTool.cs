using Editor;
using MANIFOLD.AnimGraph.Editor;
using MANIFOLD.AnimGraph.Nodes;

namespace MANIFOLD.AnimGraph.GraphTools {
    [AnimGraphTool(typeof(StateMachine))]
    public class StateMachineTool : EditorWidget {
        private StateMachine stateMachine;
        private StateMachineView stateMachineView;
        private GraphNode node;
        
        public StateMachineTool(AnimGraphEditor editor) : base(editor) {
            WindowTitle = "State Machine";
            
            Layout = Layout.Column();
        }

        public override void Open(GraphNode node) {
            this.node = node;
            
            Layout.Clear(true);
            
            stateMachine = (StateMachine)node.RealNode;
            stateMachineView = Layout.Add(new StateMachineView(stateMachine));
            stateMachineView.OnStateAdded = StateCollectionModified;
            stateMachineView.OnStateRemoved = StateCollectionModified;
            stateMachineView.OnStateSelected = InspectState;
            stateMachineView.OnTransitionSelected = InspectTransition;
        }

        private void StateCollectionModified(StateMachine.State state) {
            node.UpdatePlugs();
        }
        
        private void InspectState(StateMachine.State state) {
            editor.InspectorPanel.SetObject(state);
        }

        private void InspectTransition(StateMachine.Transition transition) {
            editor.InspectorPanel.SetObject(transition);
        }
    }
}
