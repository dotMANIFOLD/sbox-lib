using System.Collections.Generic;
using System.Linq;
using Editor;
using MANIFOLD.Jobs;
using Sandbox;

namespace MANIFOLD.JobGraph.Editor {
    public class JobGraphInspector : Widget {
        private readonly JobGraphDebugger debugger;
        private readonly ControlSheet sheet;

        public JobGraphInspector(JobGraphDebugger debugger) : base(debugger) {
            this.debugger = debugger;

            MinimumWidth = 400;
            
            Layout = Layout.Column();
            Layout.Margin = 4;
            Layout.Spacing = 4;

            sheet = new ControlSheet();
            Layout.Add(sheet);
            Layout.AddStretchCell();
        }
        
        protected override void OnPaint() {
            Paint.SetBrushAndPen(Theme.WidgetBackground);
            Paint.DrawRect(LocalRect);
        }

        public void ShowJobs(IEnumerable<IJob> jobs) {
            sheet.Clear(true);
            
            if (jobs == null) return;
            if (jobs.Count() != 1) return;
            
            var job = jobs.First();
            var serializdObj = job.GetSerialized();
            List<SerializedProperty> properties = new List<SerializedProperty>();
            foreach (var prop in serializdObj) {
                // if (prop.GetMethod == null) continue;
                if (!prop.IsPublic) continue;
                if (prop.PropertyType == typeof(IJobGraph) && prop.Name == nameof(IJob.Graph)) continue;
                if (prop.PropertyType == typeof(IReadOnlyList<IInputSocket>)) continue;
                if (prop.PropertyType == typeof(IReadOnlyList<IOutputSocket>)) continue;
                
                var value = prop.GetValue<object>();
                    
                if (job is IInputJob inputJob) {
                    if (inputJob.Inputs == value) continue;
                }
                if (job is IOutputJob outputJob) {
                    if (outputJob.Outputs == value) continue;
                    if (prop.Name == nameof(IOutputJob.OutputData)) continue;
                }
                
                properties.Add(prop);
            }
            
            sheet.AddGroup("Properties", properties.ToArray());
            
            Update();
        }

        private bool SheetFilter(SerializedProperty prop) {
            if (prop.Name == nameof(IJob.Graph) && prop.PropertyType == typeof(IJobGraph)) return false;
            return true;
        }
    }
}
