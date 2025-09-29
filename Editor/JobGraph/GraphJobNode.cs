using System;
using System.Collections.Generic;
using System.Linq;
using Editor;
using Editor.NodeEditor;
using MANIFOLD.Jobs;
using Sandbox;

namespace MANIFOLD.JobGraph.Editor {
    public abstract class GraphJobPlug : IPlug {
        public GraphNode Node { get; }
        public int PlugIndex { get; }
        
        public abstract string Identifier { get; }
        public abstract DisplayInfo DisplayInfo { get; }
        public abstract Type Type { get; }

        public bool ShowLabel => true;
        public bool AllowStretch => true;
        public bool ShowConnection => true;
        public bool InTitleBar => false;
        public bool IsReachable => true;
        public string ErrorMessage => null;

        INode IPlug.Node => Node;

        public GraphJobPlug(GraphNode node, int plugIndex) {
            Node = node;
            PlugIndex = plugIndex;
        }
        
        public ValueEditor CreateEditor(NodeUI node, Plug plug) {
            return null;
        }

        public Menu CreateContextMenu(NodeUI node, Plug plug) {
            return null;
        }

        public void OnDoubleClick(NodeUI node, Plug plug, MouseEvent e) {
            
        }
    }

    public class GraphJobPlugIn : GraphJobPlug, IPlugIn {
        private IInputSocket socket;
        private DisplayInfo displayInfo;

        public override string Identifier => $"{Node.Identifier}.In.{PlugIndex}";
        public override DisplayInfo DisplayInfo => displayInfo;
        public override Type Type => socket.DataType;

        public IPlugOut ConnectedOutput {
            get {
                if (socket.Job == null) return null;
                return Node.Wrapper.JobNodes[socket.Job.ID].Outputs.First();
            }
            set {
                Log.Warning("Can't modify connections of job graph.");
            }
        }
        
        public GraphJobPlugIn(GraphNode node, int plugIndex, IInputSocket socket) : base(node, plugIndex) {
            this.socket = socket;

            displayInfo.Name = $"Input {plugIndex}";
            displayInfo.Description = $"Input {plugIndex}";
        }

        
        public float? GetHandleOffset(string name) {
            return null;
        }

        public void SetHandleOffset(string name, float? value) {
            
        }
    }

    public class GraphJobPlugOut : GraphJobPlug, IPlugOut {
        private DisplayInfo displayInfo;
        private Type dataType;
        
        public override string Identifier => $"{Node.Identifier}.Out.{PlugIndex}";
        public override DisplayInfo DisplayInfo => displayInfo;
        public override Type Type => dataType;

        public GraphJobPlugOut(GraphJobNode node, int plugIndex) : base(node, plugIndex) {
            var outputJob = (IOutputJob)node.Job;
            // infer from output data
            dataType = outputJob.OutputData?.GetType();
            // infer from first socket
            if (dataType == null) dataType = outputJob.Outputs?.FirstOrDefault()?.DataType;
            // TODO: implement inference from generic type
            // warning
            if (dataType == null) {
                Log.Warning($"Could not infer data type for job {outputJob.ID}!");
                dataType = typeof(object);
            }
            
            displayInfo.Name = "Out";
            displayInfo.Description = "Output of this node.";
        }
    }
    
    public class GraphJobNode : GraphNode {
        protected IJob job;
        protected List<GraphJobPlugIn> inputs;
        protected List<GraphJobPlugOut> outputs;
        protected DisplayInfo displayInfo;

        public IJob Job => job;
        
        public override string Identifier => $"{job.ID}.Job";
        public override DisplayInfo DisplayInfo => displayInfo;
        
        public override IEnumerable<IPlugIn> Inputs => inputs;
        public override IEnumerable<IPlugOut> Outputs => outputs;

        public GraphJobNode(JobGraphWrapper wrapper, IJob job) : base(wrapper) {
            this.job = job;
            
            inputs = new List<GraphJobPlugIn>();
            outputs = new List<GraphJobPlugOut>();

            if (job is IInputJob inputJob) {
                for (int i = 0; i < inputJob.Inputs.Count; i++) {
                    var socket = inputJob.Inputs[i];
                    inputs.Add(new GraphJobPlugIn(this, i, socket));
                }
            }
            if (job is IOutputJob outputJob) {
                outputs.Add(new GraphJobPlugOut(this, 0));
            }
            
            displayInfo = DisplayInfo.For(job);
        }

        public override NodeUI CreateUI(GraphView view) {
            return ui = new NodeUI(view, this);
        }
    }
}
