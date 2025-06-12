using System.Linq;
using System.Text.Json.Serialization;
using MANIFOLD.Animation;
using MANIFOLD.Jobs;
using Sandbox;
using Sandbox.Diagnostics;

namespace MANIFOLD.AnimGraph {
    using Nodes;
    
    [Title("MANIFOLD Animator")]
    [Category(LibraryData.CATEGORY)]
    public class MANIFOLDAnimator : Component, Component.ExecuteInEditor {
        private SkinnedModelRenderer renderer;
        private AnimGraph graph;
        private AnimationCollection animations;
        
        private bool isPlaying;
        private JobBindData bindData;
        private JobContext context;
        
        private OrderedJobGroup mainGroup;
        private ApplyToModelJob applyJob;
        
        [Property]
        public SkinnedModelRenderer Renderer {
            get => renderer;
            set {
                renderer = value;
                Bind();
            }
        }

        [Property]
        public AnimGraph AnimGraph {
            get => graph;
            set {
                graph = value;
                mainGroup = null;
                applyJob = null;
            }
        }

        [Property]
        public AnimationCollection Animations {
            get => animations;
            set {
                animations = value;
                Bind();
            }
        }

        protected override void OnEnabled() {
            if (Scene.IsEditor) return;
            Play();
        }

        protected override void OnDisabled() {
            if (Scene.IsEditor) return;
            Stop();
        }

        protected override void OnUpdate() {
            if (isPlaying) {
                Update(Time.Delta);
            }
        }

        // PLAYING MANAGEMENT
        public void Play() {
            if (graph == null || renderer == null) return;
            if (isPlaying) return;

            context ??= new JobContext();
            
            isPlaying = true;
            context.time = 0;
        }

        public void Stop() {
            isPlaying = false;
        }

        // COMMANDS
        public void Bind() {
            if (mainGroup == null) return;
            bindData = new JobBindData(animations, renderer);
            mainGroup.BindAnimData(bindData);
            mainGroup.SetAnimContext(context);
        }
        
        public void Update(float deltaTime) {
            if (graph == null || renderer == null) return;
            
            if (mainGroup == null) {
                RebuildGraph();
            }
            
            context.time += deltaTime;
            context.deltaTime = deltaTime;
            
            using (Performance.Scope("Animation")) {
                applyJob.TraverseLeft<IBaseAnimJob, IInputAnimJob>(PrepareTraverse);
                mainGroup.Run();
            }
        }

        // UTILITY
        private void RebuildGraph() {
            mainGroup = new OrderedJobGroup();

            var jobs = graph.Nodes.Values.Select(x => x.CreateJob()).ToDictionary(x => x.ID);
            
            applyJob = (ApplyToModelJob)jobs.First(x => x.Value is ApplyToModelJob).Value;
            applyJob.SetGraph(mainGroup);
            
            // CONNECT JOBS
            foreach (var job in jobs.Values) {
                var animNode = graph.Nodes[job.ID];
                if (animNode == null) {
                    Log.Warning("Node is null???? wtf????");
                    continue;
                }
                if (job is not IInputAnimJob inputJob) {
                    continue;
                }
                
                var inputs = animNode.GetInputs();
                int index = 0;
                foreach (var reference in inputs) {
                    if (!reference.IsValid()) continue;
                    inputJob.InputFrom((IOutputAnimJob)jobs[reference.ID.Value], index);
                    index++;
                }
            }
            
            // GROUPING
            var branches = applyJob.ResolveBranchesFlat();
            foreach (var level in branches.GroupBy(x => x.depth).OrderByDescending(x => x.Key)) {
                var group = new JobGroup().SetGraph(mainGroup);
                foreach (var branch in level) {
                    branch.CreateGraph<OrderedJobGroup>().SetGraph(group);
                }
            }

            Bind();
        }
        
        private void PrepareTraverse(IBaseAnimJob job) {
            job.Prepare();
        }
    }
}
