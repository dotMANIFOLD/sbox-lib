using System;
using System.Collections;
using System.Collections.Generic;

namespace MANIFOLD.AnimGraph {
    public class ParameterList : IEnumerable<Parameter> {
        private Dictionary<Guid, Parameter> parameters;
        private Dictionary<string, Parameter> parametersByName;
        
        public ParameterList() {
            parameters = new Dictionary<Guid, Parameter>();
            parametersByName = new Dictionary<string, Parameter>();
        }

        public ParameterList(AnimGraph graph) : this() {
            foreach (var parameter in graph.Parameters.Values) {
                Add(parameter.Clone());
            }
        }

        // ADD
        public void Add<T>(Parameter<T> parameter) {
            if (parameters.ContainsKey(parameter.ID)) return;
            if (parametersByName.ContainsKey(parameter.Name)) return;
            
            parameters.Add(parameter.ID, parameter);
            parametersByName.Add(parameter.Name, parameter);
        }

        // this is hidden for type safety
        private void Add(Parameter parameter) {
            parameters.Add(parameter.ID, parameter);
            parametersByName.Add(parameter.Name, parameter);
        }
        
        // REMOVE
        public void Remove(Guid id) {
            if (!parameters.ContainsKey(id)) return;
            var param = parameters[id];
            parameters.Remove(id);
            parametersByName.Remove(param.Name);
        }

        public void Remove(string name) {
            if (!parametersByName.ContainsKey(name)) return;
            var param = parametersByName[name];
            parameters.Remove(param.ID);
            parametersByName.Remove(name);
        }

        // GET
        public Parameter Get(Guid id) {
            return parameters.GetValueOrDefault(id);
        }

        public Parameter Get(string name) {
            return parametersByName.GetValueOrDefault(name);
        }
        
        public Parameter<T> Get<T>(Guid id) {
            return parameters.GetValueOrDefault(id) as Parameter<T>;
        }

        public Parameter<T> Get<T>(string name) {
            return parametersByName.GetValueOrDefault(name) as Parameter<T>;
        }
        
        // SET
        public void Set<T>(Guid id, T value) {
            if (!parameters.ContainsKey(id)) return;
            var param = parameters[id] as Parameter<T>;
            if (param == null) return;
            param.Value = value;
        }

        public void Set<T>(string name, T value) {
            if (!parametersByName.ContainsKey(name)) return;
            var param = parametersByName[name] as Parameter<T>;
            if (param == null) return;
            param.Value = value;
        }

        public void Reset(bool autoOnly = false) {
            foreach (var param in parameters.Values) {
                if (autoOnly && !param.AutoReset) continue;
                param.Reset();
            }
        }
        
        public ParameterList Clone() {
            var clone = new ParameterList();
            foreach (var parameter in parameters.Values) {
                clone.Add(parameter.Clone());
            }
            return clone;
        }

        public IEnumerator<Parameter> GetEnumerator() {
            return parameters.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
