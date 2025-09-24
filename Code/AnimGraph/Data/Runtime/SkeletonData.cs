using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace MANIFOLD.AnimGraph {
    public class SkeletonData<T> : IEnumerable<T> {
        protected T[] data;
        protected IReadOnlyDictionary<string, int> remapTable;

        public int BoneCount => data.Length;
        
        public SkeletonData(SkeletonData<T> other) {
            data = other.data.ToArray();
            remapTable = other.remapTable;
        }

        public SkeletonData(Model model) {
            var persistentData = ModelPersistentData.Get(model);
            remapTable = persistentData.remapTable;
            data = new T[remapTable.Count];
        }
        
        public SkeletonData(IReadOnlyDictionary<string, int> remapTable) {
            this.remapTable = remapTable;
            data = new T[remapTable.Count];
        }

        public SkeletonData(T[] data, IReadOnlyDictionary<string, int> remapTable) {
            this.data = data;
            this.remapTable = remapTable;
        }
        
        public T this[int index] {
            get => data[index];
            set => data[index] = value;
        }
        
        public T this[string key] {
            get => data[remapTable[key]];
            set => data[remapTable[key]] = value;
        }

        public IEnumerator<T> GetEnumerator() {
            return (IEnumerator<T>)data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
