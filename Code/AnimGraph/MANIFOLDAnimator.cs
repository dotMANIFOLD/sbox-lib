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
    public class MANIFOLDAnimator : Component {
        [Property]
        public SkinnedModelRenderer Renderer { get; set; }
        [Property]
        public AnimationCollection Animations { get; set; }

        [Property, Range(0, 1), JsonIgnore, Change(nameof(ChangeBlend))]
        public float TestBlend { get; set; }
        
        private AnimGraph animGraph;
        
        private JobBindData bindData;
        private JobContext context;
        
        private OrderedJobGroup mainGroup;
        private ApplyToModelJob applyJob;
        private ManualBlendJob blendJob;
        
        protected override void OnStart() {
            bindData = new JobBindData(Animations, Renderer);
            context = new JobContext();

            BlendingJobGraphWithBranches();
        }

        protected override void OnUpdate() {
            context.time += Time.Delta;
            context.deltaTime = Time.Delta;
            
            using (Performance.Scope("Animation")) {
                applyJob.TraverseLeft<IBaseAnimJob, IInputAnimJob>(PrepareTraverse);
                mainGroup.Run();
            }

            Vector3 debugPos = default;
            foreach (var input in blendJob.Inputs) {
                if (input.Job is SampleJob sampler) {
                    var scope = TextRendering.Scope.Default;
                    scope.FontSize = 20;

                    scope.Text = $"{sampler.animationName} Duration: {sampler.Duration}";
                    Scene.Camera.Hud.DrawText(scope, debugPos, TextFlag.LeftTop);
                    debugPos.y += scope.FontSize * 1.2f;
                    
                    scope.Text = $"{sampler.animationName} Playback speed: {sampler.RealPlaybackSpeed}";
                    Scene.Camera.Hud.DrawText(scope, debugPos, TextFlag.LeftTop);
                    debugPos.y += scope.FontSize * 1.2f;
                }
            }
        }

        private void PrepareTraverse(IBaseAnimJob job) {
            job.Prepare();
        }

        private void ChangeBlend() {
            if (blendJob == null) return;
            // blendJob.SetWeight(0, 1 - TestBlend);
            blendJob.SetWeight(1, TestBlend);
        }

        private void SimpleJobGraph() {
            mainGroup = new OrderedJobGroup();
            
            var finalJob = new ApplyToModelJob();
            new SampleJob() {
                animationName = "@Male_Run",
                looping = true
            }.SetGraph(mainGroup).OutputTo(finalJob, 0);
            finalJob.SetGraph(mainGroup);
            
            mainGroup.BindAnimData(bindData);
            mainGroup.SetAnimContext(context);
        }
        
        private void BlendingJobGraph() {
            mainGroup = new OrderedJobGroup();
            
            var animGroup = new JobGroup()
                .SetGraph(mainGroup);

            blendJob = new ManualBlendJob(2)
                .InputFrom(new SampleJob() {
                    animationName = "@Male_Walk",
                    looping = true
                }.SetGraph(animGroup), 0)
                .InputFrom(new SampleJob() {
                    animationName = "@Male_Run",
                    looping = true
                }.SetGraph(animGroup), 1)
                .SetGraph(mainGroup);

            applyJob = new ApplyToModelJob()
                .InputFrom(blendJob, 0)
                .SetGraph(mainGroup);
            
            mainGroup.BindAnimData(bindData);
            mainGroup.SetAnimContext(context);
        }
        
        private void BlendingJobGraphWithBranches() {
            mainGroup = new OrderedJobGroup();

            blendJob = new ManualBlendJob(2)
                .InputFrom(new SampleJob() {
                    animationName = "@Male_Walk",
                    looping = true
                }, 0)
                .InputFrom(new SampleJob() {
                    animationName = "@Male_Run",
                    looping = true
                }, 1);

            applyJob = new ApplyToModelJob()
                .InputFrom(blendJob, 0);
            
            CreateMainGroupFromBranches();
            mainGroup.BindAnimData(bindData);
            mainGroup.SetAnimContext(context);
        }
        
        private void JobGraphFromAnimGraph() {
            // CreateAnimGraph();
            // 
            // jobGraph = new JobGraph();
            // finalJob = (IInputAnimJob)animGraph.FinalPoseNode.CreateJob();
            // finalJob.SetGraph(jobGraph);
            // 
            // tempChain = new OutputJobChain<Pose>();
            // CreateJobs(tempChain, animGraph, animGraph.FinalPoseNode, true);
            // ConnectJobs(tempChain, animGraph);
            // tempChain.Validate();
            // 
            // tempChain.OutputTo(finalJob, 0);
            // jobGraph.BindAnimData(bindData);
            // jobGraph.SetAnimContext(context);
        }

        private void CreateMainGroupFromBranches() {
            mainGroup = new OrderedJobGroup();
            applyJob.SetGraph(mainGroup);
            var branches = applyJob.ResolveBranchesFlat();
            foreach (var level in branches.GroupBy(x => x.depth).OrderByDescending(x => x.Key)) {
                var group = new JobGroup().SetGraph(mainGroup);
                foreach (var branch in level) {
                    branch.CreateGraph<OrderedJobGroup>().SetGraph(group);
                }
            }
        }
        
        private void CreateAnimGraph() {
            animGraph = new AnimGraph();
            var finalPose = new FinalPose();
            var animationClip = new AnimationNode() {
                animation = "@Male_Run",
                playbackSpeed = 1.2f,
                looping = true
            };
            finalPose.Pose = animationClip;
            animGraph.AddNode(finalPose);
            animGraph.AddNode(animationClip);
        }

        private void CreateJobs(IJobGraph jobGraph, AnimGraph animGraph, JobNode node, bool skipFirst) {
            // if (!skipFirst) {
            //     var job = node.CreateJob();
            //     job.SetGraph(jobGraph);
            // }
            // foreach (var socket in node.Inputs) {
            //     if (!socket.OtherNode.HasValue) continue;
            //     if (jobGraph.AllJobs.ContainsKey(socket.OtherNode.Value)) {
            //         Log.Warning($"Job {socket.OtherNode.Value} already exists! Skipping...");
            //         continue;
            //     }
            //     
            //     CreateJobs(jobGraph, animGraph, animGraph.Nodes[socket.OtherNode.Value], false);
            // }
        }

        private void ConnectJobs(IJobGraph jobGraph, AnimGraph animGraph) {
            // foreach (var job in jobGraph.AllJobs.Values) {
            //     var node = animGraph.Nodes[job.ID];
            //     for (int i = 0; i < node.Inputs.Length; i++) {
            //         var socket = node.Inputs[i];
            //         if (!socket.OtherNode.HasValue) continue;
            //         
            //         var input = (IInputAnimJob)jobGraph.AllJobs[node.ID];
            //         var output = (IOutputAnimJob)jobGraph.AllJobs[socket.OtherNode.Value];
            //         input.InputFrom(output, i);
            //     }
            // }
        }
    }
}
