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
        private NodeRef reference;
        
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

        public GraphPlugIn(GraphNode node, int plugIndex, NodeRef reference, DisplayInfo info) : base(node, plugIndex) {
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
        public enum InputOrigin { Property, Collection }
        
        public record InputData(InputOrigin Origin, NodeRef Reference, DisplayInfo Info);
        
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
            
            var nodeInputs = GetNodeInputs();
            for (int i = 0; i < nodeInputs.Count; i++) {
                var input = nodeInputs[i];
                inputs.Add(new GraphPlugIn(this, i, input.Reference, input.Info));
            }
            if (realNode is not FinalPose) {
                outputs.Add(new GraphPlugOut(this, 0));
            }
            
            DisplayInfo = DisplayInfo.For(realNode);
        }

        public void UpdatePlugs() {
            var nodeInputs = GetNodeInputs();
            int delta = nodeInputs.Count - inputs.Count;

            if (delta == 0) return;

            if (delta < 0) {
                for (int i = 0; i > delta; i--) {
                    inputs.RemoveAt(inputs.Count - 1);
                }
            } else {
                for (int i = inputs.Count; i < nodeInputs.Count; i++) {
                    var input = nodeInputs[i];
                    inputs.Add(new GraphPlugIn(this, i, input.Reference, input.Info));
                }
            }
            
            ui.MarkNodeChanged();
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

        private List<InputData> GetNodeInputs() {
            List<InputData> inputs = new List<InputData>();
            var type = realNode.GetType();

            foreach (var prop in type.GetProperties()) {
                if (prop.GetCustomAttribute<InputAttribute>() == null) continue;
                
                if (prop.PropertyType.IsAssignableTo(typeof(IEnumerable<NodeRef>))) {
                    // collection of references
                    var col = (IEnumerable<NodeRef>)prop.GetValue(realNode);
                    if (col == null) continue;
                    
                    int count = 0;
                    foreach (var reference in col) {
                        if (!reference.IsValid()) continue;
                        
                        DisplayInfo info = default;
                        info.Name = $"Slot {count}";
                        inputs.Add(new InputData(InputOrigin.Collection, reference, info));
                        count++;
                    }
                } else if (prop.PropertyType.IsAssignableTo(typeof(IEnumerable<INodeRefProvider>))) {
                    // collection of providers
                    var col = (IEnumerable<INodeRefProvider>)prop.GetValue(realNode);
                    if (col == null) continue;

                    int count = 0;
                    foreach (var provider in col) {
                        if (provider == null) continue;
                        
                        DisplayInfo info = default;
                        info.Name = $"Slot {count}";
                        inputs.Add(new InputData(InputOrigin.Collection, provider.Reference, info));
                        count++;
                    }
                } else if (prop.PropertyType.IsAssignableTo(typeof(NodeRef))) {
                    // reference
                    var reference = (NodeRef)prop.GetValue(realNode);
                    if (!reference.IsValid()) continue;
                    
                    inputs.Add(new InputData(InputOrigin.Property, reference, DisplayInfo.ForMember(prop)));
                } else if (prop.PropertyType.IsAssignableTo(typeof(INodeRefProvider))) {
                    // provider
                    var provider = (INodeRefProvider)prop.GetValue(realNode);
                    if (provider == null) continue;
                    
                    inputs.Add(new InputData(InputOrigin.Property, provider.Reference, DisplayInfo.ForMember(prop)));
                }
            }

            return inputs;
        }
    }
}
