using System;
using Editor;
using Editor.NodeEditor;
using Sandbox;

namespace MANIFOLD.AnimGraph.Editor {
    public class ClassNodeType : INodeType {
        public TypeDescription Type { get; set; }
        public DisplayInfo DisplayInfo { get; set; }

        public Menu.PathElement[] Path => Menu.GetSplitPath(DisplayInfo);

        public ClassNodeType(TypeDescription type) {
            Type = type;
            if (type == null) {
                DisplayInfo = new DisplayInfo();
            } else {
                DisplayInfo = DisplayInfo.ForType(type.TargetType);
            }
        }
        
        public bool TryGetInput(Type valueType, out string name) {
            name = "Example Input Name";
            return true;
        }

        public bool TryGetOutput(Type valueType, out string name) {
            name = "Example Output Name";
            return true;
        }

        public INode CreateNode(IGraph graph) {
            var node = new GraphNode((GraphWrapper)graph, Type.Create<JobNode>());
            return node;
        }
    }
}
