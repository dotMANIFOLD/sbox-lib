using System;
using Sandbox;

namespace MANIFOLD.Utility {
    public static class SerializedExtensions {
        public static PropertyTypeWrapper ChangeType(this SerializedProperty prop, Type newType) {
            return new PropertyTypeWrapper(prop, newType);
        }
    }
}
