using System;

namespace MANIFOLD.AnimGraph {
    [AttributeUsage(AttributeTargets.Class)]
    public class ExposeToAnimGraphAttribute : Attribute {
        
    }
    
    [AttributeUsage(AttributeTargets.Property)]
    public class InputAttribute : Attribute {
        
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class AnimationAttribute : Attribute {
        
    }
}
