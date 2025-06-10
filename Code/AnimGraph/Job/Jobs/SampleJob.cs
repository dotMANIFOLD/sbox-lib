using System;
using System.Collections.Generic;
using System.Linq;
using MANIFOLD.Jobs;
using MANIFOLD.Animation;
using Sandbox;

namespace MANIFOLD.AnimGraph {
    public class SampleJob : IOutputAnimJob {
        public class TrackGroup {
            public Track<Vector3> position;
            public Track<Rotation> rotation;
        }
        
        public string animationName;

        public float playbackSpeed = 1;
        public bool looping;
        public float time;

        internal float graphPlaybackSpeed = 1;
        
        private AnimationClip animation;
        private SkeletonData<TrackGroup> trackCache;
        private Pose cachedPose;
        private List<Output<JobResults>> outputs = new List<Output<JobResults>>();

        public Guid ID { get; }
        public IJobGraph Graph { get; set; }
        
        public JobContext Context { get; set; }
        public JobBindData BindData { get; set; }

        public IReadOnlyList<Output<JobResults>> Outputs => outputs;
        IReadOnlyList<IOutputSocket> IOutputJob.Outputs => outputs;
        
        public JobResults OutputData { get; private set; }
        object IOutputJob.OutputData => OutputData;

        public float RealPlaybackSpeed => playbackSpeed * graphPlaybackSpeed;
        public float Duration => animation.Duration * (1 / playbackSpeed);
        public float RealDuration => animation.Duration * (1 / RealPlaybackSpeed);
        
        public SampleJob() : this(Guid.NewGuid()) { }
        
        public SampleJob(Guid id) {
            ID = id;
        }
        
        public void Bind() {
            animation = BindData.animations.Animations.FirstOrDefault(x => x.Name == animationName);
            trackCache = new SkeletonData<TrackGroup>(BindData.remapTable);
            cachedPose = BindData.bindPose.Clone();
            OutputData = new JobResults(cachedPose);

            if (animation == null) return;
            
            var trackGroups = animation.Tracks.GroupBy(x => x.TargetBone);
            foreach (var tracks in trackGroups) {
                if (!BindData.remapTable.TryGetValue(tracks.Key, out int index)) continue;

                TrackGroup group = new TrackGroup();
                trackCache[index] = group;
                
                group.position = (Track<Vector3>)tracks.FirstOrDefault(x => x.Name == "LocalPosition");
                group.rotation = (Track<Rotation>)tracks.FirstOrDefault(x => x.Name == "LocalRotation");
            }
        }
        
        public void Reset() {
            time = 0;
        }

        public void Prepare() {
            
        }
        
        public void Run() {
            if (animation == null) return;
            
            float interval = (1 / animation.FrameRate);
            int frame = (time / (1 / animation.FrameRate)).FloorToInt();

            for (int i = 0; i < BindData.bindPose.BoneCount; i++) {
                Transform transform = BindData.bindPose[i];
                var group = trackCache[i];
                
                if (group.position != null) {
                    transform.Position = group.position.Get(frame);
                }
                if (group.rotation != null) {
                    transform.Rotation = group.rotation.Get(frame);
                }
                
                cachedPose[i] = transform;
            }
            time += Context.deltaTime * RealPlaybackSpeed;
            
            if (looping) {
                float duration = animation.FrameCount * interval;
                if (duration < time) {
                    float mult = (time / duration).Floor();
                    time -= duration * mult;
                }
            }
        }

        // OUTPUT API
        public void AddOutput(IInputJob<JobResults> job, int targetIndex) {
            outputs.Add(new Output<JobResults>(job, targetIndex));
        }

        void IOutputJob.AddOutput(IInputJob job, int targetIndex) {
            AddOutput((IInputJob<JobResults>)job, targetIndex);
        }
        
        public void RemoveOutput(IInputJob<JobResults> job, int targetIndex) {
            if (targetIndex <= -1) {
                outputs.Remove(outputs.First(o => o.Job == job));
            } else {
                outputs.Remove(outputs.First(o => o.Job == job && o.Index == targetIndex));
            }
        }

        void IOutputJob.RemoveOutput(IInputJob job, int targetIndex) {
            RemoveOutput((IInputJob<JobResults>)job, targetIndex);
        }
        
        public void DisconnectOutputs() {
            while (outputs.Count > 0) {
                this.RemoveOutput(0);
            }
        }
    }
}
