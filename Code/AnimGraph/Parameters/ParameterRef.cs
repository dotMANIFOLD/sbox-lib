using System;
using System.Text.Json.Serialization;
using Sandbox;

namespace MANIFOLD.AnimGraph {
    /// <summary>
    /// Serializable reference to a parameter.
    /// </summary>
    /// <param name="ID">ID of the parameter.</param>
    /// <typeparam name="T">Expected value type of the parameter.</typeparam>
    public record struct ParameterRef<T>(Guid? ID) : IValid {
        [JsonIgnore]
        public bool IsValid => ID.HasValue;
        
        public static implicit operator ParameterRef<T>(Parameter param) {
            if (param == null) return default;
            return new ParameterRef<T>(param.ID);
        }

        public override string ToString() {
            return ID.ToString();
        }
    }
}
