using System.Linq;
using MANIFOLD.Animation;
using MANIFOLD.Jobs;
using Sandbox;
using Sandbox.Diagnostics;

namespace MANIFOLD.AnimGraph {
    using Jobs;
    
    [Title("MANIFOLD Animator")]
    [Category(LibraryData.CATEGORY)]
    public class MANIFOLDAnimator : Component, Component.ExecuteInEditor {
        private SkinnedModelRenderer renderer;
        private AnimGraph graph;
        private AnimGraphResources animations;
        
        private bool isPlaying;
        private JobBindData bindData;
        private JobContext context;
        private ParameterList parameters;
        
        private OrderedJobGroup mainGroup;
        private ApplyToModelJob applyJob;
        
        /// <summary>
        /// Model to animate.
        /// <remarks>Automatically rebinds.</remarks>
        /// </summary>
        [Property]
        public SkinnedModelRenderer Renderer {
            get => renderer;
            set {
                if (renderer == value) return;
                renderer = value;
                Bind();
            }
        }

        /// <summary>
        /// AnimGraph to use.
        /// <remarks>Automatically rebuilds and rebinds.</remarks>
        /// </summary>
        [Property]
        public AnimGraph AnimGraph {
            get => graph;
            set {
                if (graph == value) return;
                graph = value;
                mainGroup = null;
                applyJob = null;
                parameters = null;
            }
        }

        /// <summary>
        /// Animations to use.
        /// <remarks>This doesn't have to match the AnimGraph default.</remarks>
        /// </summary>
        [Property]
        public AnimGraphResources Animations {
            get => animations;
            set {
                animations = value;
                Bind();
            }
        }
        
        /// <summary>
        /// Access to this animator's parameters.
        /// </summary>
        public ParameterList Parameters => parameters;
        
        public bool IsPlaying => isPlaying;

        protected override void OnEnabled() {
            if (Scene.IsEditor) return;
            Play();
        }

        protected override void OnDisabled() {
            if (Scene.IsEditor) return;
            Stop();
        }

        protected override void OnDestroy() {
            mainGroup?.RemoveFromDebug();
        }

        protected override void OnUpdate() {
            // TODO: move updating to the scene system
            if (isPlaying) {
                Update(Time.Delta);
            }
        }

        // PLAYING MANAGEMENT
        /// <summary>
        /// Starts playing animations.
        /// </summary>
        public void Play() {
            if (graph == null || renderer == null) return;
            if (isPlaying) return;
            
            isPlaying = true;
            if (context != null) {
                context.time = 0;
            }
        }

        /// <summary>
        /// Stops playing animations.
        /// </summary>
        public void Stop() {
            isPlaying = false;
        }

        // COMMANDS
        /// <summary>
        /// Binds this animator to the model.
        /// </summary>
        public void Bind() {
            if (mainGroup == null) return;
            bindData = new JobBindData(animations, renderer);
            context ??= new JobContext();
            mainGroup.BindAnimData(bindData);
            mainGroup.SetAnimContext(context);
        }
        
        /// <summary>
        /// Update the animator by <paramref name="deltaTime"/>
        /// </summary>
        /// <param name="deltaTime">Time passed</param>
        public void Update(float deltaTime) {
            if (graph == null || renderer == null) return;
            
            if (mainGroup == null) {
                if (parameters == null) {
                    RebuildParameters();
                }
                RebuildGraph();
            }
            
            context.time += deltaTime;
            context.deltaTime = deltaTime;
            
            using (Performance.Scope("Animation")) {
                applyJob.TraverseLeft<IBaseAnimJob, IInputAnimJob>(PrepareTraverse);
                mainGroup.Run();
                parameters.Reset(true);
            }
        }
        
        /// <summary>
        /// Rebuilds the underlying job graph. Shouldn't have to be called unless the AnimGraph was modified.
        /// </summary>
        public void RebuildGraph() {
            if (graph == null) return;

            if (mainGroup != null) {
                mainGroup.RemoveFromDebug();
            }
            
            mainGroup = new OrderedJobGroup();

            // CREATE APPLY TO MODEL JOB
            applyJob = new ApplyToModelJob();
            applyJob.SetGraph(mainGroup);
            
            // CREATE ANIM GRAPH
            JobCreationContext ctx = new JobCreationContext();
            ctx.parameters = parameters;

            var animGraphJob = new AnimGraphJob(graph, ctx);
            animGraphJob.OutputTo(applyJob, 0);
            
            // GROUPING
            var branches = applyJob.ResolveBranchesFlat();
            foreach (var level in branches.GroupBy(x => x.depth).OrderByDescending(x => x.Key)) {
                var group = new JobGroup().SetGraph(mainGroup);
                foreach (var branch in level) {
                    branch.CreateGraph<OrderedJobGroup>().SetGraph(group);
                }
            }

            Bind();
            
            mainGroup.AddToDebug(GameObject.Name + "_Animator");
        }

        /// <summary>
        /// Rebuilds the underlying parameter list. Shouldn't have to be called unless the AnimGraph's parameters was modified.
        /// </summary>
        public void RebuildParameters() {
            if (graph == null) return;
            parameters = new ParameterList(graph);
            parameters.Reset();
        }
        
        private void PrepareTraverse(IBaseAnimJob job) {
            job.Prepare();
        }
    }
}
