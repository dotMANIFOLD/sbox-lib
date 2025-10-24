using System;
using System.Collections.Generic;
using System.Linq;
using MANIFOLD.Animation;
using MANIFOLD.Jobs;
using Sandbox;
using Sandbox.Diagnostics;
using SoundEvent = MANIFOLD.Animation.SoundEvent;

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
        private TagList tags;
        
        private OrderedJobGroup mainGroup;
        private AnimGraphJob graphJob;
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
        /// <summary>
        /// Access to this animator's tags.
        /// </summary>
        public TagList TagList => tags;

        /// <summary>
        /// Whether this animator should automatically update. If false, you can update it manually via <see cref="Update"/>
        /// </summary>
        public bool AutoUpdate { get; set; }
        
        public Action<GenericEvent> OnGenericEvent { get; set; }
        public Action<FootstepEvent> OnFootstepEvent { get; set; }
        public Action<SoundEvent> OnSoundEvent { get; set; }
        public Action<BodyGroupEvent> OnBodyGroupEvent { get; set; }
        
        public bool IsPlaying => isPlaying;

        protected override void OnAwake() {
            OnSoundEvent = OnSoundEventDefault;
            OnBodyGroupEvent = OnBodyGroupEventDefault;

            AutoUpdate = true;
            
            RebuildParameters();
            RebuildTags();
        }

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
                if (parameters == null) RebuildParameters();
                if (tags == null) RebuildTags();
                RebuildGraph();
            }
            
            context.time += deltaTime;
            context.deltaTime = deltaTime;
            
            using (Performance.Scope("Animation")) {
                applyJob.TraverseLeft<IBaseAnimJob, IInputAnimJob>(PrepareTraverse);
                mainGroup.Run();
                parameters.Reset(true);

                var results = applyJob.LastResult;
                if (results.TriggeredEvents != null) {
                    foreach (var evt in results.TriggeredEvents) {
                        if (evt is GenericEvent genericEvent) {
                            OnGenericEvent?.Invoke(genericEvent);
                        } else if (evt is FootstepEvent footstepEvent) {
                            OnFootstepEvent?.Invoke(footstepEvent);
                        } else if (evt is SoundEvent soundEvent) {
                            OnSoundEvent?.Invoke(soundEvent);
                        } else if (evt is BodyGroupEvent bodyGroupEvent) {
                            OnBodyGroupEvent?.Invoke(bodyGroupEvent);
                        }
                    }
                }
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
            
            // CREATE ANIM GRAPH
            JobCreationContext ctx = new JobCreationContext();
            ctx.model = Renderer.Model;
            ctx.resources = animations;
            ctx.parameters = parameters;
            ctx.tags = tags;

            graphJob = new AnimGraphJob(graph, ctx);
            graphJob.SetGraph(mainGroup);

            // CREATE APPLY TO MODEL JOB
            applyJob = new ApplyToModelJob();
            applyJob.SetGraph(mainGroup);
            applyJob.InputFrom(graphJob, 0);
            
            Bind();
            applyJob.Reset();
            
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

        /// <summary>
        /// Rebuilds the underlying tag list. Shouldn't have to be called unless the AnimGraph's tags were modified.
        /// </summary>
        public void RebuildTags() {
            if (graph == null) return;
            tags = new TagList();
            tags.AddGraph(graph);
        }

        public IBaseAnimJob GetAccessibleJob(string str) {
            return graphJob.GetAccessibleJob(str);
        }
        
        private void PrepareTraverse(IBaseAnimJob job) {
            job.Prepare();
        }

        private void OnSoundEventDefault(SoundEvent evt) {
            Vector3 position = WorldPosition;
            if (evt.Attachment != null) {
                var result = Renderer.GetAttachment(evt.Attachment);
                if (result.HasValue) {
                    position = result.Value.Position;
                }
            }
            Sound.Play(evt.Event, position);
        }

        private void OnBodyGroupEventDefault(BodyGroupEvent evt) {
            Renderer.SetBodyGroup(evt.BodyGroup, evt.Value);
        }
    }
}
