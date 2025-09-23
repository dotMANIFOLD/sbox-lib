using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json.Serialization;
using Sandbox;

namespace MANIFOLD.AnimGraph {
    public static class JobCategories {
        public const string FINAL_COLOR = "#eace1b";
        
        public const string SAMPLING = "Sampling";
        public const string SAMPLING_COLOR = "#b02bea";

        public const string BLEND = "Blend";
        public const string BLEND_COLOR = "#44c83f";

        public const string MODIFIER = "Modifier";
        public const string MODIFIER_COLOR = "#ee8013";
    }

    public class JobCreationContext {
        public Model model;
        public AnimGraphResources resources;
        public ParameterList parameters;
    }
    
    public abstract class JobNode : BaseNode {
        /// <summary>
        /// Is this node reachable?
        /// </summary>
        [Hide, ReadOnly]
        public bool Reachable { get; set; }
        
        [Hide, JsonIgnore]
        public abstract Color AccentColor { get; }
        
        public abstract IBaseAnimJob CreateJob(in JobCreationContext ctx);
        public abstract IEnumerable<NodeRef> GetInputs();
    }
}
