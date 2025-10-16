using System;

namespace MANIFOLD.AnimGraph.Editor {
    [AttributeUsage(AttributeTargets.Class)]
    public class AnimGraphToolAttribute : Attribute {
        public Type targetType;
        
        public AnimGraphToolAttribute(Type targetType) {
            this.targetType = targetType;
        }
    }
}
