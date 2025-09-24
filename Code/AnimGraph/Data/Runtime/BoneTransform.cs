using System;
using System.Collections.Generic;

namespace MANIFOLD.AnimGraph {
    public class BoneTransform {
        private readonly BoneTransform parent;
        private readonly List<BoneTransform> children;
        
        private Transform localTransform;
        private Transform modelTransform;

        public Transform LocalTransform {
            get => localTransform;
            set {
                localTransform = value;
                LocalToModelOperation();
            }
        }

        public Transform ModelTransform {
            get => modelTransform;
            set {
                modelTransform = value;
                ModelToLocalOperation();
            }
        }

        public BoneTransform(BoneTransform parent = null) {
            this.parent = parent;
            children = new List<BoneTransform>();
            
            localTransform = Transform.Zero;
            modelTransform = Transform.Zero;
            
            if (parent != null) {
                parent.children.Add(this);
            }
        }

        public void CopyFrom(BoneTransform other) {
            localTransform = other.localTransform;
            modelTransform = other.modelTransform;
        }

        private void LocalToModelOperation() {
            if (parent != null) {
                modelTransform = parent.modelTransform.ToWorld(localTransform);
            } else {
                modelTransform = localTransform;
            }

            foreach (var child in children) {
                child.LocalToModelOperation();
            }
        }

        private void ModelToLocalOperation() {
            if (parent != null) {
                localTransform = parent.modelTransform.ToLocal(modelTransform);
            } else {
                localTransform = modelTransform;
            }

            foreach (var child in children) {
                child.LocalToModelOperation();   
            }
        }
    }
}
