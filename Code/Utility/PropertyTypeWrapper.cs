using System;
using Sandbox;

namespace MANIFOLD.Utility {
    public class PropertyTypeWrapper : SerializedProperty {
        private readonly SerializedProperty property;
        private readonly Type newType;

        public override SerializedObject Parent => property.Parent;
        public override bool IsProperty => property.IsProperty;
        public override bool IsField => property.IsField;
        public override bool IsMethod => property.IsMethod;
        public override string Name => property.Name;
        public override string DisplayName => property.DisplayName;
        public override string Description => property.Description;
        public override string GroupName => property.GroupName;
        public override int Order => property.Order;
        public override bool IsEditable => property.IsEditable;
        public override bool IsPublic => property.IsPublic;
        public override Type PropertyType => newType;
        public override bool IsValid => property.IsValid;
        public override string SourceFile => property.SourceFile;
        public override int SourceLine => property.SourceLine;
        public override bool HasChanges => property.HasChanges;

        public PropertyTypeWrapper(SerializedProperty property, Type newType) {
            this.property = property;
            this.newType = newType;
        }
        
        public override void SetValue<T>(T value) {
            property.SetValue(value);
        }

        public override T GetValue<T>(T defaultValue = default(T)) {
            return property.GetValue(defaultValue);
        }

        public override bool TryGetAsObject(out SerializedObject obj) {
            var value = property.GetValue<object>();
            obj = TypeLibrary.GetSerializedObject(value);
            return obj != null;
        }
    }
}
