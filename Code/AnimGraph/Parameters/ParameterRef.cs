using System;
using System.Text.Json.Serialization;
using Sandbox;

namespace MANIFOLD.AnimGraph {
    /// <summary>
    /// Please use <see cref="ParameterRef{T}"/>
    /// </summary>/
    public record ParameterRef(Guid? ID) : IValid {
        public Guid? ID { get; set; } = ID;
        
        [JsonIgnore]
        public bool IsValid => ID.HasValue;

        public ParameterRef() : this((Guid?)null) {
            
        }
        
        public static implicit operator ParameterRef(Parameter param) {
            if (param == null) return default;
            return new ParameterRef(param.ID);
        }

        public override string ToString() {
            return ID.ToString();
        }
    }

    /// <summary>
    /// Serializable reference to a parameter.
    /// </summary>
    /// <param name="ID">ID of the parameter.</param>
    /// <typeparam name="T">Expected value type of the parameter.</typeparam>
    public record ParameterRef<T>(Guid? ID) : ParameterRef(ID) {
        public ParameterRef() : this((Guid?)null) {
            
        }
    }
}
