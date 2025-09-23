using System;

namespace MANIFOLD.AnimGraph {
    [AttributeUsage(AttributeTargets.Class)]
    public class ExposeToAnimGraphAttribute : Attribute {
        public string Color { get; set; } = "#ececec";
    }
    
    [AttributeUsage(AttributeTargets.Property)]
    public class InputAttribute : Attribute {
        
    }
}
