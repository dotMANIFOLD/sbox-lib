using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Sandbox;

namespace MANIFOLD.AnimGraph {
    /// <summary>
    /// Serializable reference to another job node.
    /// </summary>
    public struct JobNodeReference : IValid {
        public Guid? OtherNode { get; set; }

        [JsonIgnore]
        public bool IsValid => OtherNode.HasValue;

        public static implicit operator JobNodeReference(JobNode node) {
            if (node == null) return default;
            
            return new JobNodeReference() {
                OtherNode = node.ID
            };
        }
    }
    
    public abstract class JobNode : BaseNode {
        public Type Type => GetType();
        
        public abstract IBaseAnimJob CreateJob();
    }
}
