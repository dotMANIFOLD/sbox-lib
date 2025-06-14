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
            
        }

        public void Prepare() {
            
        }
        
        public void Run() {
            if (!BindData.target.IsValid()) return;
            Pose pose = inputs[0].Job.OutputData.Pose;
            SetBone(pose, BindData.target.Model.Bones.Root, Transform.Zero);
        }
        
        private void SetBone(Pose pose, BoneCollection.Bone bone, Transform parent) {
            Transform result = Transform.Concat(parent, pose[bone.Name]);
            BindData.target.SetBoneTransform(bone, result);
            foreach (var child in bone.Children) {
                SetBone(pose, child, result);
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
