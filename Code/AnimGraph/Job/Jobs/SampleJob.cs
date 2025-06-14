﻿using System;
using System.Collections.Generic;
using System.Linq;
using MANIFOLD.Jobs;
using MANIFOLD.Animation;
using Sandbox;

namespace MANIFOLD.AnimGraph.Jobs {
    public class SampleJob : IOutputAnimJob {
        private class TrackGroup {
            public Track<Vector3> position;
            public Track<Rotation> rotation;
        }
        
        private string animationName;
        private AnimationClip animation;
        
        private Parameter<float> speedParameter;
        private float playbackSpeed = 1;
        internal float graphPlaybackSpeed = 1;
        
        private SkeletonData<TrackGroup> trackCache;
        private Pose cachedPose;
        private List<Output<JobResults>> outputs = new List<Output<JobResults>>();

        public Guid ID { get; }
        public IJobGraph Graph { get; set; }
        
        public JobContext Context { get; set; }
        public JobBindData BindData { get; set; }

        public string Animation {
            get => animationName;
            set {
                animationName = value;
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
        public float Time { get; set; }
        
        public IReadOnlyList<Output<JobResults>> Outputs => outputs;
        IReadOnlyList<IOutputSocket> IOutputJob.Outputs => outputs;
        
        public JobResults OutputData { get; private set; }
        object IOutputJob.OutputData => OutputData;

        public float RealPlaybackSpeed => PlaybackSpeed * graphPlaybackSpeed;
        public float Duration => animation.Duration * (1 / PlaybackSpeed);
        public float RealDuration => animation.Duration * (1 / RealPlaybackSpeed);
        
        public SampleJob() : this(Guid.NewGuid()) { }
        
        public SampleJob(Guid id) {
            ID = id;
        }
        
        public void Bind() {
            trackCache = new SkeletonData<TrackGroup>(BindData.remapTable);
            cachedPose = BindData.bindPose.Clone();
            OutputData = new JobResults(cachedPose);

            CacheAnimation();
        }
        
        public void Reset() {
            Time = 0;
        }

        public void Prepare() {
            
        }
        
        public void Run() {
            if (animation == null || trackCache == null) return;
            
            float interval = (1 / animation.FrameRate);
            int frame = (Time / (1 / animation.FrameRate)).FloorToInt();

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
            Time += Context.deltaTime * RealPlaybackSpeed;
            
            if (Looping) {
                float duration = animation.FrameCount * interval;
                if (duration < Time) {
                    float mult = (Time / duration).Floor();
                    Time -= duration * mult;
                }
            }
        }

        private void CacheAnimation() {
            if (trackCache == null) return;
            
            animation = BindData.animations.Animations.FirstOrDefault(x => x.Name == animationName);
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
