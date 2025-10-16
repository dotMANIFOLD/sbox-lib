using System;
using System.Collections.Generic;
using System.Linq;
using Editor;
using Editor.NodeEditor;
using Sandbox;

namespace MANIFOLD.AnimGraph.Editor {
    public class AnimGraphView : GraphView {
        public readonly AnimGraphEditor editor;
        private List<INodeType> availableNodes;
        
        public AnimGraphView(AnimGraphEditor editor) : base(editor) {
            this.editor = editor;
            editor.OnGraphReload += OnGraphReload;

            Name = "GraphView";
            WindowTitle = "Node Graph";

            base.OnSelectionChanged += OnSelectionChanged;
            
            availableNodes = EditorTypeLibrary
                .GetTypesWithAttribute<ExposeToAnimGraphAttribute>()
                .Where(x => x.Type.TargetType.IsAssignableTo(typeof(JobNode)))
                .Select(x => (INodeType)new ClassNodeType(x.Type))
                .ToList();
        }
        
        protected override IEnumerable<INodeType> GetRelevantNodes(NodeQuery query) {
            return availableNodes;
        }

        private new void OnSelectionChanged() {
            editor.SelectedNodes = SelectedItems
                .OfType<JobNodeUI>()
                .Select(x => (GraphNode)x.Node)
                .Select(x => x.RealNode);
        }
        
        private void OnGraphReload() {
            Graph = editor.GraphWrapper;
            RebuildFromGraph();
        }
    }
}
