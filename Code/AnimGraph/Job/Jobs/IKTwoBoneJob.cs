using System;
using MANIFOLD.Jobs;

namespace MANIFOLD.AnimGraph.Jobs {
    // Algorithm by Ryan Juckett
    // Analytic Two-Bone IK in 2D: https://www.ryanjuckett.com/analytic-two-bone-ik-in-2d/
    
    // Reference implementation by Guillaume Blanc
    // ozz_animation ik_two_bone_job.cc: https://github.com/guillaumeblanc/ozz-animation/blob/master/src/animation/runtime/ik_two_bone_job.cc
    
    /// <summary>
    /// IK for two bones only.
    /// </summary>
    public class IKTwoBoneJob : AnimJob {
        private string rootBone;
        private string midBone;
        private string tipBone;

        private bool needsRebind;
        private bool validChain;

        public string RootBone {
            get => rootBone;
            set {
                rootBone = value;
                needsRebind = true;
            }
        }
        public string MidBone {
            get => midBone;
            set {
                midBone = value;
                needsRebind = true;
            }
        }
        public string TipBone {
            get => tipBone;
            set {
                tipBone = value;
                needsRebind = true;
            }
        }

        public Vector3 Target { get; set; }
        
        public IKTwoBoneJob() : base() {
            inputs = new Input<JobResults>[1];
        }

        public IKTwoBoneJob(Guid id) : base(id) {
            inputs = new Input<JobResults>[1];
        }
        
        public override void Bind() {
            
        }

        public override void Run() {
            Pose inputPose = inputs[0].Job.OutputData.Pose;
            Transform transRoot = inputPose[rootBone];
            Transform transMid = inputPose[midBone];
            Transform transTip = inputPose[tipBone];
            float lengthRootMid = transRoot.Position.Distance(transMid.Position);
            float lengthMidTip = transMid.Position.Distance(transTip.Position);
            float lengthRootTarget = transRoot.Position.Distance(Target);
        }
    }
}
