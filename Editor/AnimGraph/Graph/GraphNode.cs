using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Editor;
using Editor.NodeEditor;
using Facepunch.ActionGraphs;
using Sandbox;
using DisplayInfo = Sandbox.DisplayInfo;

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
        private DisplayInfo displayInfo;
        private NodeReference reference;
        
        public override string Identifier => $"{Node.Identifier}.In.{PlugIndex}";
        public override DisplayInfo DisplayInfo => displayInfo;

        public IPlugOut ConnectedOutput {
            get {
                if (!reference.IsValid) return null;
                return Node.Graph.Nodes[reference.ID.Value].Outputs.First();
            }
            set {
                var node = ((GraphNode)value?.Node)?.RealNode;
                reference.ID = node?.ID;
                Log.Info($"Set connnection: {node?.ID}");
            }
        }

        public GraphPlugIn(GraphNode node, int plugIndex, NodeReference reference, DisplayInfo info) : base(node, plugIndex) {
            this.reference = reference;
            this.displayInfo = info;
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
        protected JobNodeUI ui;
        protected JobNode realNode;
        protected List<GraphPlugIn> inputs;
        protected List<GraphPlugOut> outputs;

        public GraphWrapper Graph => graph;
        public JobNode RealNode => realNode;
        public string Identifier => realNode.ID.ToString();
        public virtual DisplayInfo DisplayInfo { get; }
        public Color PrimaryColor { get; set; } = Color.White;
        public Color AccentColor { get; set; } = Color.White;

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
        public bool CanRemove => realNode is not FinalPose;

        public Pixmap Thumbnail => null;

        public virtual string ErrorMessage => null;
        
        public event Action Changed;

        public GraphNode(GraphWrapper graph, JobNode realNode) {
            this.graph = graph;
            this.realNode = realNode;
            
            inputs = new List<GraphPlugIn>();
            outputs = new List<GraphPlugOut>();
            
            DisplayInfo = DisplayInfo.For(realNode);
            
            Refresh();
        }

        public void Refresh() {
            inputs.Clear();
            outputs.Clear();
            
            var nodeType = realNode.GetType();
            int count = 0;
            
            foreach (var prop in nodeType.GetProperties()) {
                if (prop.GetCustomAttribute<InputAttribute>() == null) continue;
                
                Type propType;
                if (prop.PropertyType.IsArray) {
                    int rank = prop.PropertyType.GetArrayRank();
                    if (rank != 1) {
                        Log.Warning("Multi dimensional arrays are not supported.");
                        continue;
                    }
                    
                    propType = prop.PropertyType.GetElementType();
                } else {
                    propType = prop.PropertyType;
                }
                
                bool isReference = propType.IsAssignableTo(typeof(NodeReference));
                bool isProvider = propType.IsAssignableTo(typeof(INodeReferenceProvider));
                
                if (!isReference && !isProvider) {
                    Log.Warning("Invalid input type.");
                    continue;
                }
                
                NodeReference GetReference(object obj) {
                    if (obj == null) return null;
                    if (isReference) {
                        return (NodeReference)obj;
                    } else {
                        return ((INodeReferenceProvider)obj).Reference;
                    }
                }
                
                if (prop.PropertyType.IsArray) {
                    var arr = (Array)prop.GetValue(realNode);
                    if (arr == null) continue;
                    
                    for (int i = 0; i < arr.Length; i++) {
                        var reference = GetReference(arr.GetValue(i));
                        if (reference == null) continue;

                        var displayInfo = new DisplayInfo();
                        displayInfo.Name = $"Slot {count}";
                        
                        inputs.Add(new GraphPlugIn(this, count, reference, displayInfo));
                        count++;
                    }
                } else {
                    var reference = GetReference(prop.GetValue(realNode));
                    if (reference == null) continue;
                    
                    inputs.Add(new GraphPlugIn(this, count, reference, DisplayInfo.ForMember(prop)));
                    count++;  
                }
            }
            if (realNode is not FinalPose) {
                outputs.Add(new GraphPlugOut(this, 0));
            }

            if (ui != null) {
                ui.MarkNodeChanged();
            }
        }
        
        public virtual void OnPaint(Rect rect) {
            
        }

        public virtual void OnDoubleClick(MouseEvent e) {
            
        }
        
        public virtual NodeUI CreateUI(GraphView view) {
            return ui = new JobNodeUI(view, this);
        }

        public virtual Color GetPrimaryColor(GraphView view) {
            return PrimaryColor;
        }

        public virtual Color GetAccentColor(GraphView view) {
            return realNode.AccentColor;
        }

        public virtual Menu CreateContextMenu(NodeUI node) {
            return null;
        }
    }
}
