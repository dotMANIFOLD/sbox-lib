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
        private readonly GraphNode node;
        private DisplayInfo displayInfo;
        private SerializedObject obj;
        
        public override string Identifier => $"{Node.Identifier}.In.{PlugIndex}";
        public override DisplayInfo DisplayInfo => displayInfo;

        public IPlugOut ConnectedOutput {
            get {
                if (!obj.IsValid) return null;
                var id = IDProperty.GetValue<Guid?>();
                if (!id.HasValue) return null;
                return Node.Graph.Nodes[id.Value].Outputs.First();
            }
            set {
                var jobNode = ((GraphNode)value?.Node)?.RealNode;
                IDProperty.SetValue(jobNode?.ID);
                node.Graph.ScanReachableNodes();
                Log.Info($"Set connnection: {jobNode?.ID}");
            }
        }

        private SerializedProperty IDProperty => obj.GetProperty("ID");

        public GraphPlugIn(GraphNode node, int plugIndex, SerializedObject obj, DisplayInfo info) : base(node, plugIndex) {
            this.node = node;
            this.obj = obj;
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
        
        public record InputData(InputOrigin Origin, SerializedObject Object, DisplayInfo Info);
        
        protected GraphWrapper graph;
        protected JobNodeUI ui;
        protected JobNode realNode;
        protected SerializedObject serialized;
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
        public bool IsReachable => realNode.Reachable;

        public bool CanClone => realNode is not FinalPose;
        public bool CanRemove => realNode is not FinalPose;

        public Pixmap Thumbnail => null;

        public virtual string ErrorMessage => null;
        
        public event Action Changed;

        public GraphNode(GraphWrapper graph, JobNode realNode) {
            this.graph = graph;
            this.realNode = realNode;
            serialized = realNode.GetSerialized();
            
            inputs = new List<GraphPlugIn>();
            outputs = new List<GraphPlugOut>();
            
            var nodeInputs = GetNodeInputs();
            for (int i = 0; i < nodeInputs.Count; i++) {
                var input = nodeInputs[i];
                inputs.Add(new GraphPlugIn(this, i, input.Object, input.Info));
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
                    inputs.Add(new GraphPlugIn(this, i, input.Object, input.Info));
                }
            }
            
            ui.MarkNodeChanged();
        }
        
        public virtual void OnPaint(Rect rect) {
            
        }

        public virtual void OnDoubleClick(MouseEvent e) {
            graph.editor.OpenTool(this);
        }
        
        public virtual NodeUI CreateUI(GraphView view) {
            return ui = new JobNodeUI(view, this);
        }

        public virtual string GetDisplayName() {
            return realNode.DisplayName;
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
                bool valid = false;
                
                var attr = prop.GetCustomAttribute<InputAttribute>();
                valid = attr != null;
                if (!valid) continue;
                var serializedProp = serialized.GetProperty(prop.Name);
                
                bool success = serializedProp.TryGetAsObject(out SerializedObject serializedObj);
                if (!success) {
                    Log.Warning($"Failed to convert property: {prop.Name}");
                    continue;
                }
                
                if (prop.PropertyType.IsAssignableTo(typeof(IEnumerable<NodeRef>))) {
                    // collection of references
                    var serializedCol = (SerializedCollection)serializedObj;
                    
                    int count = 0;
                    foreach (var colProp in serializedCol) {
                        success = colProp.TryGetAsObject(out SerializedObject refObj);
                        if (!success) {
                            Log.Warning($"Failed to convert property in collection: {prop.Name}");
                            continue;
                        }
                        
                        DisplayInfo info = default;
                        info.Name = $"Slot {count}";
                        inputs.Add(new InputData(InputOrigin.Collection, refObj, info));
                        count++;
                    }
                } else if (prop.PropertyType.IsAssignableTo(typeof(IEnumerable<INodeRefProvider>))) {
                    // collection of providers
                    var serializedCol = (SerializedCollection)serializedObj;
                    
                    int count = 0;
                    foreach (var colProp in serializedCol) {
                        success = colProp.TryGetAsObject(out SerializedObject providerObj);
                        if (!success) {
                            Log.Warning($"Failed to convert property in collection: {prop.Name}");
                            continue;
                        }
                        var provider = colProp.GetValue<INodeRefProvider>();
                        
                        success = providerObj.GetProperty(provider.RefFieldName).TryGetAsObject(out SerializedObject refObj);
                        if (!success) {
                            Log.Warning($"Failed to convert provider property in collection: {prop.Name}");
                            continue;
                        }
                        
                        DisplayInfo info = default;
                        info.Name = $"Slot {count}";
                        if (provider is INameProvider nameProvider) {
                            if (nameProvider.Name != "Unnamed") info.Name = nameProvider.Name;
                        }
                        
                        inputs.Add(new InputData(InputOrigin.Collection, refObj, info));
                        count++;
                    }
                } else if (prop.PropertyType.IsAssignableTo(typeof(NodeRef))) {
                    // reference
                    inputs.Add(new InputData(InputOrigin.Property, serializedObj, DisplayInfo.ForMember(prop)));
                } else if (prop.PropertyType.IsAssignableTo(typeof(INodeRefProvider))) {
                    // provider
                    string propName = typeof(INodeRefProvider).FullName + "." + nameof(INodeRefProvider.Reference);
                    success = serializedObj.GetProperty(propName).TryGetAsObject(out SerializedObject refObj);
                    if (!success) {
                        Log.Warning($"Failed to convert provider property in collection: {prop.Name}");
                        continue;
                    }
                    
                    DisplayInfo info = DisplayInfo.ForMember(prop);
                    if (prop.GetValue(realNode) is INameProvider nameProvider) {
                        if (nameProvider.Name != "Unnamed") info.Name = nameProvider.Name;
                    }
                    inputs.Add(new InputData(InputOrigin.Property, refObj, DisplayInfo.ForMember(prop)));
                } else {
                    Log.Warning("No matching type for this input!");
                }
            }

            return inputs;
        }
    }
}
