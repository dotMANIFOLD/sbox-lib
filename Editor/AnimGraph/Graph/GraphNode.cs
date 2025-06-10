using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Editor;
using Editor.NodeEditor;
using Sandbox;

namespace MANIFOLD.AnimGraph.Editor {
    using Nodes;
    
    public abstract class GraphPlug : IPlug {
        public GraphNode Node { get; }
        public int PlugIndex { get; }
        
        public abstract string Identifier { get; }
        public abstract DisplayInfo DisplayInfo { get; }
        
        public bool ShowLabel => true;
        public bool AllowStretch => true;
        public bool ShowConnection => true;
        public bool InTitleBar => false;
        public bool IsReachable => true;
        
        public string ErrorMessage => null;
        
        INode IPlug.Node => Node;
        Type IPlug.Type => typeof(Pose);

        public GraphPlug(GraphNode node, int plugIndex) {
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

    public class GraphPlugIn : GraphPlug, IPlugIn {
        private PropertyInfo info;
        private DisplayInfo displayInfo;
        
        public override string Identifier => $"{Node.Identifier}.In.{PlugIndex}";
        public override DisplayInfo DisplayInfo => displayInfo;

        public IPlugOut ConnectedOutput {
            get {
                var reference = (JobNodeReference)info.GetValue(Node.RealNode);
                if (!reference.IsValid) return null;

                return Node.Graph.Nodes[reference.OtherNode.Value].Outputs.First();
            }
            set {
                info.SetValue(Node.RealNode, (JobNodeReference)(((GraphNode)value?.Node)?.RealNode));
            }
        }

        public GraphPlugIn(GraphNode node, int plugIndex, PropertyInfo info) : base(node, plugIndex) {
            this.info = info;
            displayInfo = DisplayInfo.ForMember(info);
        }
        
        public float? GetHandleOffset(string name) {
            return null;
        }

        public void SetHandleOffset(string name, float? value) {
            
        }
    }

    public class GraphPlugOut : GraphPlug, IPlugOut {
        private DisplayInfo displayInfo = default;
        
        public override string Identifier => $"{Node.Identifier}.Out.{PlugIndex}";
        public override DisplayInfo DisplayInfo => displayInfo;

        public GraphPlugOut(GraphNode node, int plugIndex) : base(node, plugIndex) {
            displayInfo.Name = "Out";
            displayInfo.Description = "Output of this node.";
        }
    }
    
    public class GraphNode : INode {
        protected GraphWrapper graph;
        protected JobNode realNode;
        protected List<GraphPlugIn> inputs;
        protected List<GraphPlugOut> outputs;
        protected List<JobNodeReference> realInputs;

        public GraphWrapper Graph => graph;
        public JobNode RealNode => realNode;
        public string Identifier => realNode.ID.ToString();
        public virtual DisplayInfo DisplayInfo { get; }

        public Vector2 Position {
            get => realNode.Position;
            set => realNode.Position = value;
        }
        public bool AutoSize => false;
        public Vector2 ExpandSize { get; set; }
        
        public virtual IEnumerable<IPlugIn> Inputs => inputs;
        public virtual IEnumerable<IPlugOut> Outputs => outputs;
        public bool HasTitleBar => true;
        public bool IsReachable => true;

        public bool CanClone => true;
        public bool CanRemove => true;

        public Pixmap Thumbnail => null;

        public virtual string ErrorMessage => null;
        
        public event Action Changed;

        public GraphNode(GraphWrapper graph, JobNode realNode) {
            this.graph = graph;
            this.realNode = realNode;
            
            inputs = new List<GraphPlugIn>();
            outputs = new List<GraphPlugOut>();
            realInputs = new List<JobNodeReference>();

            var nodeType = realNode.GetType();
            int count = 0;
            foreach (var prop in nodeType.GetProperties()) {
                if (prop.GetCustomAttribute<InputAttribute>() is { } attr) {
                    inputs.Add(new GraphPlugIn(this, count, prop));
                    realInputs.Add((JobNodeReference)prop.GetValue(realNode));
                    count++;
                }
            }
            if (realNode is not FinalPose) {
                outputs.Add(new GraphPlugOut(this, 0));
            }
            
            DisplayInfo = DisplayInfo.For(realNode);
        }
        
        public virtual void OnPaint(Rect rect) {
            
        }

        public virtual void OnDoubleClick(MouseEvent e) {
            
        }
        
        public virtual NodeUI CreateUI(GraphView view) {
            return new NodeUI(view, this);
        }

        public virtual Color GetPrimaryColor(GraphView view) {
            return Color.White;
        }

        public virtual Menu CreateContextMenu(NodeUI node) {
            return null;
        }
    }
}
