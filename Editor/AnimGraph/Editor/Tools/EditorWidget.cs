using System;
using Editor;

namespace MANIFOLD.AnimGraph.Editor {
    public abstract class EditorWidget : Widget {
        public readonly AnimGraphEditor editor;

        public Action OnDestroyedEvent;
        
        public EditorWidget(AnimGraphEditor editor) : base(null) {
            this.editor = editor;
        }

        public override void OnDestroyed() {
            base.OnDestroyed();
            OnDestroyedEvent?.Invoke();
        }

        public abstract void Open(GraphNode data);
    }
}
