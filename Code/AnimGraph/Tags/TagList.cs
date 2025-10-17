using System;
using System.Collections;
using System.Collections.Generic;

namespace MANIFOLD.AnimGraph {
    public class TagList : IEnumerable<Tag> {
        private Dictionary<Guid, Tag> tags;
        private Dictionary<string, Tag> tagsByName;

        public TagList() {
            tags = new Dictionary<Guid, Tag>();
            tagsByName = new Dictionary<string, Tag>();
        }
        
        // ADD
        public void AddGraph(AnimGraph graph) {
            foreach (var tag in graph.Tags.Values) {
                Add(tag.Clone());
            }
        }

        public void Add(Tag tag) {
            if (tags.ContainsKey(tag.ID)) return;
            if (tagsByName.ContainsKey(tag.Name)) {
                Log.Info($"Can't add tag with duplicat name: {tag.Name}");
                return;
            }
            
            tags.Add(tag.ID, tag);
            tagsByName.Add(tag.Name, tag);
        }

        // REMOVE
        public void Remove(Guid id) {
            if (!tags.ContainsKey(id)) return;
            var tag = tags[id];
            tags.Remove(id);
            tagsByName.Remove(tag.Name);
        }

        public void Remove(string name) {
            if (!tagsByName.ContainsKey(name)) return;
            var tag = tagsByName[name];
            tags.Remove(tag.ID);
            tagsByName.Remove(name);
        }
        
        // GET
        public Tag Get(Guid id) {
            return tags.GetValueOrDefault(id);
        }

        public Tag Get(string name) {
            return tagsByName.GetValueOrDefault(name);
        }
        
        public IEnumerator<Tag> GetEnumerator() {
            return tags.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
