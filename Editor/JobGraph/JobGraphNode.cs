using System;
using System.Collections.Generic;
using System.Linq;
using Editor;
using Editor.NodeEditor;
using MANIFOLD.Jobs;
using Sandbox;

namespace MANIFOLD.JobGraph.Editor {
    public abstract class JobGraphPlug : IPlug {
        public JobGraphNode Node { get; }
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

        public JobGraphPlug(JobGraphNode node, int plugIndex) {
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

    public class JobGraphPlugIn : JobGraphPlug, IPlugIn {
        private IInputSocket socket;
        private DisplayInfo displayInfo;

        public override string Identifier => $"{Node.Identifier}.In.{PlugIndex}";
        public override DisplayInfo DisplayInfo => displayInfo;
        public override Type Type => socket.DataType;

        public IPlugOut ConnectedOutput {
            get {
                if (socket.Job == null) return null;
                return Node.Wrapper.AllNodes[socket.Job.ID].Outputs.First();
            }
            set {
                Log.Warning("Can't modify connections of job graph.");
            }
        }
        
        public JobGraphPlugIn(JobGraphNode node, int plugIndex, IInputSocket socket) : base(node, plugIndex) {
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

    public class JobGraphPlugOut : JobGraphPlug, IPlugOut {
        private DisplayInfo displayInfo;
        private Type dataType;
        
        public override string Identifier => $"{Node.Identifier}.Out.{PlugIndex}";
        public override DisplayInfo DisplayInfo => displayInfo;
        public override Type Type => dataType;

        public JobGraphPlugOut(JobGraphNode node, int plugIndex) : base(node, plugIndex) {
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
    
    public class JobGraphNode : INode {
        protected JobGraphWrapper wrapper;
        protected NodeUI ui;
        protected IJob job;
        protected List<JobGraphPlugIn> inputs;
        protected List<JobGraphPlugOut> outputs;

        public JobGraphWrapper Wrapper => wrapper;
        public IJob Job => job;
        public string Identifier => job.ID.ToString();
        public virtual DisplayInfo DisplayInfo { get; }
        
        public Vector2 Position { get; set; }
        public bool AutoSize => false;
        public Vector2 ExpandSize { get; set; }

        public virtual IEnumerable<IPlugIn> Inputs => inputs;
        public virtual IEnumerable<IPlugOut> Outputs => outputs;

        public bool HasTitleBar => true;
        public bool IsReachable => true;
        public bool CanClone => false;
        public bool CanRemove => false;

        public Pixmap Thumbnail => null;

        public virtual string ErrorMessage => null;
        
        public event Action Changed;

        public JobGraphNode(JobGraphWrapper wrapper, IJob job) {
            this.wrapper = wrapper;
            this.job = job;
            
            inputs = new List<JobGraphPlugIn>();
            outputs = new List<JobGraphPlugOut>();

            if (job is IInputJob inputJob) {
                for (int i = 0; i < inputJob.Inputs.Count; i++) {
                    var socket = inputJob.Inputs[i];
                    inputs.Add(new JobGraphPlugIn(this, i, socket));
                }
            }
            if (job is IOutputJob outputJob) {
                outputs.Add(new JobGraphPlugOut(this, 0));
            }
            
            DisplayInfo = DisplayInfo.For(job);
        }

        public void Update(int zIndex) {
            ui.ZIndex = zIndex;
            ui.MarkNodeChanged();
        }
        
        public void OnPaint(Rect rect) {
            
        }
        
        public void OnDoubleClick(MouseEvent e) {
            
        }
        
        public virtual NodeUI CreateUI(GraphView view) {
            if (job is IJobGraph subGraph) {
                var contains = subGraph.Select(x => wrapper.AllNodes[x.ID].ui);
                ui = new JobGraphGroupUI(view, this, contains);
            } else {
                ui = new NodeUI(view, this);
            }
            return ui;
        }

        public Color GetPrimaryColor(GraphView view) {
            return Color.Gray;
        }

        public Menu CreateContextMenu(NodeUI node) {
            return null;
        }
    }
}
