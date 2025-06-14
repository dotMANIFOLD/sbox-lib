using System;
using System.Linq;
using System.Threading.Tasks;
using Editor;
using Editor.NodeEditor;

namespace MANIFOLD.AnimGraph.Editor {
    [EditorForAssetType(AnimGraph.EXTENSION)]
    public class AnimGraphEditor : Window, IAssetEditor {
        private StatusBar statusBar;
        private DockManager dock;
        
        private Preview preview;
        private Inspector inspector;
        private ParameterTable parameterTable;
        private TagList tagList;
        private AnimGraphView graphView;
        
        public Asset GraphAsset { get; set; }
        public AnimGraph GraphResource { get; set; }
        
        public bool CanOpenMultipleAssets => false;

        public AnimGraphEditor() {
            WindowTitle = "MANIFOLD AnimGraph";
            Size = new Vector2(1600, 800);
            
            SetWindowIcon("sports_gymnastics");
            CreateMenuBar();

            Canvas = new Widget(this);
            Canvas.Layout = Layout.Column();
            Canvas.Layout.Spacing = 4;
            
            statusBar = new StatusBar(Canvas);
            statusBar.MinimumHeight = 40;
            statusBar.SetSizeMode(SizeMode.Flexible, SizeMode.CanGrow);
            
            dock = new DockManager(Canvas);
            dock.SetSizeMode(SizeMode.Flexible, SizeMode.Flexible);
            
            Canvas.Layout.Add(statusBar);
            Canvas.Layout.Add(dock);
            
            DefaultDockState();
            
            Show();
        }
        
        public void AssetOpen(Asset asset) {
            if (asset == null || string.IsNullOrWhiteSpace(asset.AbsolutePath)) return;
            
            GraphAsset = asset;
            GraphResource = GraphAsset.LoadResource<AnimGraph>();
            
            graphView.Graph = new GraphWrapper(GraphResource);
            statusBar.Graph = GraphResource;
            parameterTable.Graph = GraphResource;
            preview.Graph = GraphResource;
            
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
        }

        private void DefaultDockState() {
            dock.Clear();

            preview = new Preview();
            inspector = new Inspector();
            parameterTable = new ParameterTable();
            tagList = new TagList();
            graphView = new AnimGraphView(this);
            
            inspector.OnInputChanged += OnPropertyChanged;
            graphView.OnSelectionChanged += OnSelectionChanged;
            
            dock.RegisterDockType("Preview", null, () => preview = new Preview());
            dock.RegisterDockType("Inspector", null, () => inspector = new Inspector());
            dock.RegisterDockType("ParameterList", null, () => parameterTable = new ParameterTable());
            dock.RegisterDockType("TagList", null, () => tagList = new TagList());
            
            dock.AddDock(null, graphView, DockArea.Right, DockManager.DockProperty.HideCloseButton);
            dock.AddDock(graphView, preview, DockArea.Left, DockManager.DockProperty.HideOnClose, split: 0.2f);
            dock.AddDock(graphView, inspector, DockArea.Right, DockManager.DockProperty.HideOnClose, split: 0.2f);
            dock.AddDock(preview, parameterTable, DockArea.Bottom, DockManager.DockProperty.HideOnClose, split: 0.4f);
            dock.AddDock(parameterTable, tagList, DockArea.Inside, DockManager.DockProperty.HideOnClose);

            dock.RaiseDock(parameterTable);
            
            dock.Update();
        }

        private void OnSelectionChanged() {
            inspector.SetNodes(graphView.SelectedItems.Cast<NodeUI>().Select(x => x.Node).Cast<GraphNode>());
        }

        private void OnPropertyChanged() {
            foreach (var node in graphView.SelectedItems.Cast<NodeUI>().Select(x => x.Node).Cast<GraphNode>()) {
                node.UpdatePlugs();
            }
        }
        
        // FILE MANAGEMENT
        private void SaveGraph() {
            Log.Info("Save called");
        }

        private void CloseSelf() {
            Log.Info("Close called");
        }
    }
}
