using System;

namespace MANIFOLD.AnimGraph {
    /// <summary>
    /// Exposes this class to the AnimGraph.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ExposeToAnimGraphAttribute : Attribute {
        public string Color { get; set; } = "#ececec";
    }
    
    /// <summary>
    /// Marks this property as an input. Only works for <see cref="NodeRef"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class InputAttribute : Attribute { }
    
    /// <summary>
    /// Changes to properties with this attribute should modify node inputs.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class UpdatesInputsAttribute : Attribute { }
}
