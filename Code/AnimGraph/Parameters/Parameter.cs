using System;
using System.Text.Json.Serialization;
using Sandbox;

namespace MANIFOLD.AnimGraph {
    public abstract class Parameter {
        public Guid ID { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "Unnamed Parameter";
        public bool AutoReset { get; set; }

        [Hide, JsonIgnore]
        public Action OnChanged { get; set; }
        
        [Hide, JsonIgnore]
        public abstract object ObjectValue { get; }
        [Hide, JsonIgnore]
        public abstract Type DataType { get; }
        
        public abstract void Reset();
        public abstract Parameter Clone();
    }

    public class Parameter<T> : Parameter {
        protected T backingField;
        
        [Space]
        public T DefaultValue { get; set; }
        
        [JsonIgnore]
        public virtual T Value {
            get => backingField;
            set {
                backingField = value;
                OnChanged?.Invoke();
            }
        }

        [Hide, JsonIgnore]
        public sealed override object ObjectValue => backingField;
        [Hide, JsonIgnore]
        public sealed override Type DataType => typeof(T);

        public override void Reset() {
            backingField = DefaultValue;
            OnChanged?.Invoke();
        }

        public override Parameter Clone() {
            return new Parameter<T>() {
                ID = ID,
                Name = Name,
                AutoReset = AutoReset,
                backingField = backingField,
                DefaultValue = DefaultValue
            };
        }
    }
}
