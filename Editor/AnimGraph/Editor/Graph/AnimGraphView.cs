using System.Collections.Generic;
using System.Linq;
using Editor;
using Editor.NodeEditor;

namespace MANIFOLD.AnimGraph.Editor {
    public class AnimGraphView : GraphView {
        private List<INodeType> availableNodes;
        
        public AnimGraphView(Widget parent) : base(parent) {
            WindowTitle = "Graph";
            
            availableNodes = TypeLibrary
                .GetTypesWithAttribute<ExposeToAnimGraphAttribute>()
                .Where(x => x.Type.TargetType.IsAssignableTo(typeof(JobNode)))
                .Select(x => (INodeType)new ClassNodeType(x.Type))
                .ToList();
        }

        protected override IEnumerable<INodeType> GetRelevantNodes(NodeQuery query) {
            return availableNodes;
        }
    }
}
