using System;
using Editor;

namespace MANIFOLD.AnimGraph.Editor {
    [EditorForAssetType("manimg")]
    [EditorApp("MANIFOLD AnimGraph", "sports_gymnastics", "Awesomer anim graph")]
    public class AnimGraphEditor : DockWindow, IAssetEditor {
        private AnimGraphView graphView;
        
        public Asset GraphAsset { get; set; }
        public AnimGraph GraphResource { get; set; }
        
        public bool CanOpenMultipleAssets => false;

        public AnimGraphEditor() {
            WindowTitle = "MANIFOLD AnimGraph";
            Size = new Vector2(1600, 800);
            
            GraphResource = AnimGraph.DefaultPreset();
            
            SetWindowIcon("sports_gymnastics");
            CreateMenuBar();
            CreateWindows();
            
            Show();
        }
        
        public void AssetOpen(Asset asset) {
            if (asset == null || string.IsNullOrWhiteSpace(asset.AbsolutePath)) return;
            
            GraphAsset = asset;
            GraphResource = GraphAsset.LoadResource<AnimGraph>();
            
            Focus();
            EditorEvent.Run(EditorEvents.OPEN_EVENT);
        }

        public void SelectMember(string memberName) {
            throw new NotImplementedException(); // what the hell does this do
        }

        private void CreateMenuBar() {
            var file = MenuBar.AddMenu("File");
            file.Enabled = false;
        }
        
        private void CreateWindows() {
            graphView = new AnimGraphView(this);
            graphView.Graph = new GraphWrapper(GraphResource);
            
            DockManager.AddDock(null, graphView);
        }
    }
}
