using System;
using System.Collections.Generic;
using Editor;
using Editor.NodeEditor;
using MANIFOLD.Jobs;
using Sandbox;

namespace MANIFOLD.JobGraph.Editor {
    public class GraphGroupNode : GraphNode {
        protected Guid id;
        protected IJobGraph graph;
        protected DisplayInfo displayInfo;

        public IEnumerable<GraphicsItem> Containing { get; set; }
        public Color GroupColor { get; set; }
        
        public override string Identifier => $"{id}.Group";
        public override DisplayInfo DisplayInfo => displayInfo;

        public override IEnumerable<IPlugIn> Inputs => [];
        public override IEnumerable<IPlugOut> Outputs => [];
        
        public GraphGroupNode(JobGraphWrapper wrapper, IJobGraph graph, Guid id) : base(wrapper) {
            this.id = id;
            this.graph = graph;

            displayInfo = DisplayInfo.For(graph);
        }
        
        public override NodeUI CreateUI(GraphView view) {
            return ui = new GraphGroupUI(view, this);
        }
    }
}
