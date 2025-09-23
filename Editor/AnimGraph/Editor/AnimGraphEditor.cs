using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Editor;
using Editor.NodeEditor;

namespace MANIFOLD.AnimGraph.Editor {
    [EditorForAssetType(AnimGraph.EXTENSION)]
    public class AnimGraphEditor : Window, IAssetEditor {
        public const string CONTEXT_GRAPH = "MANIFOLD_AnimGraph_Graph";
        public const string EVENT_REBUILD = "manifold.animgraph.editor.rebuild";
        
        private DockManager dock;
        
        private PreviewPanel previewPanel;
        private InspectorPanel inspectorPanel;
        private ResourcePanel resourcePanel;
        private ParameterTable parameterTable;
        private TagList tagList;
        private AnimGraphView graphView;

        private Asset graphAsset;
        private AnimGraph graphResource;
        private GraphWrapper graphWrapper;
        
        private IEnumerable<BaseNode> selectedNodeses;
        private IEnumerable<Guid> selectedNodeGuids;
        private Parameter selectedParameter;
        private Guid? selectedParameterGuid;
        
        public Asset GraphAsset => graphAsset;
        public AnimGraph GraphResource => graphResource;
        public GraphWrapper GraphWrapper => graphWrapper;

        public IEnumerable<BaseNode> SelectedNodes {
            get => selectedNodeses;
            set {
                var oldParam = selectedParameter;
                
                selectedNodeses = value;
                selectedNodeGuids = value.Select(x => x.ID);
                selectedParameter = null;
                selectedParameterGuid = null;
                
                inspectorPanel.SetNodes(value);
                parameterTable.OnSelectionChanged(oldParam, null);
            }
        }
        public Parameter SelectedParameter {
            get => selectedParameter;
            set {
                var oldParam = selectedParameter;
                
                selectedParameter = value;
                selectedParameterGuid = selectedParameter?.ID;
                selectedNodeses = null;
                selectedParameterGuid = null;
                
                graphView.ClearSelection();
                inspectorPanel.SetParameter(value);
                parameterTable.OnSelectionChanged(oldParam, value);
            }
        }
        
        public bool InPreview { get; set; }
        public bool ShowDebugInfo { get; set; }
        
        public event Action OnGraphReload;
        
        public bool CanOpenMultipleAssets => false;
        
        public AnimGraphEditor() {
            WindowTitle = "MANIFOLD AnimGraph";
            Size = new Vector2(1600, 800);
            
            SetWindowIcon("sports_gymnastics");
            CreateMenuBar();

            Canvas = new Widget(this);
            Canvas.Layout = Layout.Column();
            Canvas.Layout.Spacing = 4;
            
            dock = new DockManager(Canvas);
            dock.SetSizeMode(SizeMode.Flexible, SizeMode.Flexible);
            
            Canvas.Layout.Add(dock);
            
            DefaultDockState();
            
            Show();
        }
        
        public void AssetOpen(Asset asset) {
            if (asset == null || string.IsNullOrWhiteSpace(asset.AbsolutePath)) return;
            
            graphAsset = asset;
            graphResource = GraphAsset.LoadResource<AnimGraph>();
            graphWrapper = new GraphWrapper(GraphResource);
            
            SetContext(CONTEXT_GRAPH, graphResource);
            OnGraphReload?.Invoke();
            Focus();
        }

        public void SelectMember(string memberName) {
            throw new NotImplementedException(); // what the hell does this do
        }

        private void CreateMenuBar() {
            {
                var file = MenuBar.AddMenu("File");
                file.AddOption("Save", null, SaveGraph, "editor.save");
                file.AddSeparator();
                file.AddOption("Quit", null, CloseSelf, "editor.quit");
            }

            {
                var edit = MenuBar.AddMenu("Edit");
                edit.AddOption("Undo", "undo", null, "editor.undo");
                edit.AddOption("Redo", "redo", null, "editor.redo");
                edit.AddSeparator();
                edit.AddOption("Cut", "cut", null, "editor.cut");
                edit.AddOption("Copy", "copy", null, "editor.copy");
                edit.AddOption("Paste", "paste", null, "editor.paste");
            }

            {
                var view = MenuBar.AddMenu("View");
                var debugOption = view.AddOption("Show Debug Info", "bug_report");
                debugOption.Checkable = true;
                debugOption.Toggled = (value) => ShowDebugInfo = value;
            }
        }

        private void DefaultDockState() {
            dock.Clear();

            previewPanel = new PreviewPanel(this);
            inspectorPanel = new InspectorPanel(this);
            resourcePanel = new ResourcePanel(this);
            parameterTable = new ParameterTable(this);
            tagList = new TagList();
            graphView = new AnimGraphView(this);
            
            inspectorPanel.OnNodeInputChanged += OnInputChanged;
            
            dock.RegisterDockType("Preview", null, () => previewPanel = new PreviewPanel(this));
            dock.RegisterDockType("Inspector", null, () => inspectorPanel = new InspectorPanel(this));
            dock.RegisterDockType("ParameterList", null, () => parameterTable = new ParameterTable(this));
            dock.RegisterDockType("TagList", null, () => tagList = new TagList());
            
            dock.AddDock(null, graphView, DockArea.Right, DockManager.DockProperty.HideCloseButton);
            dock.AddDock(graphView, previewPanel, DockArea.Left, DockManager.DockProperty.HideOnClose, split: 0.2f);
            dock.AddDock(graphView, inspectorPanel, DockArea.Right, DockManager.DockProperty.HideOnClose, split: 0.2f);
            dock.AddDock(inspectorPanel, resourcePanel, DockArea.Inside, DockManager.DockProperty.HideOnClose);
            dock.RaiseDock(inspectorPanel);
            dock.AddDock(previewPanel, parameterTable, DockArea.Bottom, DockManager.DockProperty.HideOnClose, split: 0.4f);
            dock.AddDock(parameterTable, tagList, DockArea.Inside, DockManager.DockProperty.HideOnClose);

            dock.RaiseDock(parameterTable);
            
            dock.Update();
        }

        // CHANGES
        private void OnInputChanged() {
            foreach (var node in graphView.SelectedItems.Cast<NodeUI>().Select(x => x.Node).Cast<GraphNode>()) {
                node.UpdatePlugs();
            }
        }
        
        // FILE MANAGEMENT
        private void SaveGraph() {
            graphAsset.SaveToDisk(GraphResource);
            graphResource = GraphAsset.LoadResource<AnimGraph>();
            graphWrapper = new GraphWrapper(GraphResource);

            if (selectedNodeGuids != null) {
                selectedNodeses = selectedNodeGuids.Select(x => graphResource.Nodes[x]);
            }
            if (selectedParameterGuid.HasValue) {
                selectedParameter = graphResource.Parameters[selectedParameterGuid.Value];
            }
            
            OnGraphReload?.Invoke();
            Log.Info("Anim graph saved");
        }

        private void CloseSelf() {
            Log.Info("Close called");
        }
    }
}
