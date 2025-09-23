using System.Linq;
using Editor;
using Editor.NodeEditor;
using MANIFOLD.Jobs;

namespace MANIFOLD.JobGraph.Editor {
    [EditorApp("Job Graph Debugger", "account_tree", "Debug job graphs")]
    public class JobGraphDebugger : Window {
        private readonly ComboBox graphList;
        private readonly Splitter splitter;
        private readonly GraphView graphView;
        private readonly JobGraphInspector inspector;

        private JobGraphWrapper wrapper;
        
        public JobGraphDebugger() {
            WindowTitle = "Job Graph Debugger";
            Size = new Vector2(1600, 800);

            Canvas = new Widget(this);
            Canvas.Layout = Layout.Column();

            var row = Canvas.Layout.AddRow();
            row.Margin = 2;
            row.Spacing = 4;
            graphList = row.Add(new ComboBox(Canvas));
            row.Add(new IconButton("refresh") { OnClick = RefreshGraphList });
            row.AddSeparator();
            row.Add(new Button("Organize"));
            row.AddStretchCell();

            splitter = Canvas.Layout.Add(new Splitter(Canvas));
            graphView = new GraphView(splitter);
            graphView.OnSelectionChanged = OnGraphSelectionChanged;
            inspector = new JobGraphInspector(this);
            splitter.AddWidget(graphView);
            splitter.AddWidget(inspector);
            
            RefreshGraphList();
            Show();
        }

        [EditorEvent.Frame]
        private void OnFrame() {
            wrapper?.GroupUpdate();
        }
        
        public void ShowGraph(IJobGraph graph) {
            if (graph == null) {
                graphView.Graph = null;
                return;
            }

            wrapper = new JobGraphWrapper(graph);
            graphView.Graph = wrapper;
        }
        
        private void RefreshGraphList() {
            graphList.Clear();
            graphList.AddItem("<None>", enabled: false);
            foreach (var entry in JobGraphDebug.Graphs) {
                var result = entry.graph.TryGetTarget(out var graph);
                if (!result) continue;
                
                graphList.AddItem(entry.name, onSelected: () => {
                    ShowGraph(graph);
                });
            }
        }

        private void OnGraphSelectionChanged() {
            var jobs = graphView.SelectedItems
                .Cast<NodeUI>()
                .Select(x => (JobGraphNode)x.Node)
                .Select(x => x.Job);
            inspector.ShowJobs(jobs);
        }
    }
}
