using System;
using System.Collections.Generic;
using System.Linq;
using MANIFOLD.Jobs;
using Sandbox;

namespace MANIFOLD.AnimGraph.Jobs {
    public sealed class ApplyToModelJob : IInputAnimJob {
        private Input<JobResults>[] inputs = [ null ];

        public Guid ID { get; } = Guid.AllBitsSet;
        public IJobGraph Graph { get; set; }
        
        public JobContext Context { get; set; }
        public JobBindData BindData { get; set; }
        
        public IReadOnlyList<Input<JobResults>> Inputs => inputs;
        IReadOnlyList<IInputSocket> IInputJob.Inputs => inputs;

        public void Bind() {
            
        }
        
        public void Reset() {
            var animJob = Inputs[0].Job as IAnimJob;
            animJob?.Reset();
        }

        public void Prepare() {
            
        }
        
        public void Run() {
            if (!BindData.target.IsValid()) return;
            Pose pose = inputs[0].Job?.OutputData.Pose ?? BindData.bindPose;

            for (int i = 0; i < pose.BoneCount; i++) {
                BindData.target.SetBoneTransform(BindData.target.Model.Bones.AllBones[i], pose[i].ModelTransform);
            }
        }
        
        // INPUT API
        public void SetInput(int index, IOutputJob<JobResults> job) {
            inputs[index] = new Input<JobResults>(job);
        }

        void IInputJob.SetInput(int index, IOutputJob job) {
            SetInput(index, (IOutputJob<JobResults>)job);
        }
        
        public void DisconnectInputs() {
            inputs[0] = null;
        }
    }
}
