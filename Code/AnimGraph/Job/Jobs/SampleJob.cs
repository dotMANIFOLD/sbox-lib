using System;
using System.Collections.Generic;
using System.Linq;
using MANIFOLD.Jobs;
using MANIFOLD.Animation;
using Sandbox;

namespace MANIFOLD.AnimGraph.Jobs {
    public class SampleJob : IOutputAnimJob {
        private class TrackGroup {
            public BoneTrack<Vector3> position;
            public BoneTrack<Rotation> rotation;
        }

        private AnimationClip clip;
        
        private Parameter<float> speedParameter;
        private float playbackSpeed = 1;
        internal float graphPlaybackSpeed = 1;
        
        private SkeletonData<TrackGroup> trackCache;
        private Pose workingPose;
        private List<Output<JobResults>> outputs = new List<Output<JobResults>>();

        public Guid ID { get; }
        public IJobGraph Graph { get; set; }
        
        public JobContext Context { get; set; }
        public JobBindData BindData { get; set; }

        public AnimationClip Clip {
            get => clip;
            set {
                clip = value;
                CacheAnimation();
            }
        }

        public float PlaybackSpeed {
            get => PlaybackSpeedParameter?.Value ?? playbackSpeed;
            set {
                playbackSpeed = value;
                speedParameter = null;
            }
        }
        public Parameter<float> PlaybackSpeedParameter {
            get => speedParameter;
            set {
                if (speedParameter != null && value == null) {
                    playbackSpeed = speedParameter.Value;
                }
                speedParameter = value;
            }
        }
        
        public bool Looping { get; set; }
        public bool Interpolate { get; set; }
        public float Time { get; set; }
        
        public IReadOnlyList<Output<JobResults>> Outputs => outputs;
        IReadOnlyList<IOutputSocket> IOutputJob.Outputs => outputs;
        
        public JobResults OutputData { get; private set; }
        object IOutputJob.OutputData => OutputData;

        public float RealPlaybackSpeed => PlaybackSpeed * graphPlaybackSpeed;
        public float Duration => clip.Duration * (1 / PlaybackSpeed);
        public float RealDuration => clip.Duration * (1 / RealPlaybackSpeed);
        
        public SampleJob() : this(Guid.NewGuid()) { }
        
        public SampleJob(Guid id) {
            ID = id;
        }
        
        public void Bind() {
            trackCache = new SkeletonData<TrackGroup>(BindData.remapTable);
            workingPose = BindData.bindPose.Clone();

            CacheAnimation();
        }
        
        public void Reset() {
            Time = 0;
        }

        public void Prepare() {
            
        }
        
        public void Run() {
            if (clip == null || trackCache == null) return;
            
            float interval = (1 / clip.FrameRate);
            float frame = Time / (1 / clip.FrameRate);
            int lastFrame = frame.FloorToInt();
            float lerpFactor = frame - lastFrame;

            for (int i = 0; i < workingPose.BoneCount; i++) {
                BoneTransform transform = workingPose[i];
                Transform local = transform.LocalTransform;
                var group = trackCache[i];
                
                if (group.position != null) {
                    local.Position = group.position.Get(lastFrame);
                    if (Interpolate) {
                        var next = group.position.GetNext(lastFrame);
                        local.Position = local.Position.LerpTo(next, lerpFactor);
                    }
                }
                if (group.rotation != null) {
                    local.Rotation = group.rotation.Get(lastFrame);
                    if (Interpolate) {
                        var next = group.rotation.GetNext(lastFrame);
                        local.Rotation = local.Rotation.SlerpTo(next, lerpFactor);
                    }
                }

                transform.LocalTransform = local;
            }
            Time += Context.deltaTime * RealPlaybackSpeed;
            
            if (Looping) {
                float duration = clip.FrameCount * interval;
                if (duration < Time) {
                    float mult = (Time / duration).Floor();
                    Time -= duration * mult;
                }
            }

            OutputData = new JobResults(workingPose, Time / clip.FrameRate, !Looping && Time >= clip.Duration);
        }

        private void CacheAnimation() {
            if (clip == null || trackCache == null) return;
            clip.Load();
            
            var trackGroups = clip.BoneTracks.GroupBy(x => x.TargetBone);
            foreach (var tracks in trackGroups) {
                if (!BindData.remapTable.TryGetValue(tracks.Key, out int index)) continue;

                TrackGroup group = new TrackGroup();
                trackCache[index] = group;
                
                group.position = (BoneTrack<Vector3>)tracks.FirstOrDefault(x => x.Name == "LocalPosition");
                group.rotation = (BoneTrack<Rotation>)tracks.FirstOrDefault(x => x.Name == "LocalRotation");
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
