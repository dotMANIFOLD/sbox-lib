using System;
using Sandbox;

namespace MANIFOLD.Inspector {
    [AttributeUsage(AttributeTargets.Property)]
    public class HideIfValidAttribute : ConditionalVisibilityAttribute {
        public string PropertyName { get; set; }

        public HideIfValidAttribute(string propertyName) {
            PropertyName = propertyName;
        }
        
        public override bool TestCondition(SerializedObject so) {
            if (so.TryGetProperty(PropertyName, out var prop)) {
                var value = prop.GetValue<IValid>(prop);
                return value.IsValid();
            }
            return false;
        }

        public override bool TestCondition(object targetObject, TypeDescription td) {
            throw new NotImplementedException();
        }
    }
}
