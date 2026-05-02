using System;
using Sandbox;

namespace MANIFOLD.Inspector {
    /// <summary>
    /// Like the <see cref="ChangeAttribute"/>. But only applied with valid tools.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ToolChangeAttribute(string name) : Attribute {
        public readonly string name = name;

        public ToolChangeAttribute() : this(null) {
        }
    }
}
