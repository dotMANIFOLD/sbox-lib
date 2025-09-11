using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Nodes;
using Sandbox;

namespace MANIFOLD.Utility {
    public static class PolymorphicExtensions {
        public const string TYPE_FIELD = "__type";
        public const string NULL_KEY = "__null";

        public static JsonNode ToPolymorphic<T>(this T obj) {
            JsonNode node = Json.ToNode(obj);
            node[TYPE_FIELD] = Json.ToNode(obj.GetType(), typeof(Type));
            return node;
        }

        public static T FromPolymorphic<T>(this JsonNode node) {
            var type = Json.FromNode<Type>(node[TYPE_FIELD]);
            return (T)Json.FromNode(node, type);
        }
        
        public static JsonArray SerializePolymorphic<T>(this IList<T> source) {
            JsonArray arr = new JsonArray();
            foreach (T item in source) {
                arr.Add(item.ToPolymorphic());
            }
            return arr;
        }

        public static JsonObject SerializePolymorphic<TKey, TValue>(this IDictionary<TKey, TValue> source) {
            JsonObject obj = new JsonObject();
            foreach (var pair in source) {
                string key = pair.Key?.ToString() ?? NULL_KEY;
                obj[key] = pair.Value.ToPolymorphic();
            }
            return obj;
        }

        public static List<T> DeserializePolymorphic<T>(this JsonArray arr, Action<T> onCreate = null) {
            List<T> list = new List<T>();
            foreach (var node in arr) {
                var value = node.FromPolymorphic<T>();
                list.Add(value);
                onCreate?.Invoke(value);
            }
            return list;
        }

        public static Dictionary<TKey, TValue> DeserializePolymorphic<TKey, TValue>(this JsonObject obj, Action<TKey, TValue> onCreate = null) {
            Dictionary<TKey, TValue> dict = new Dictionary<TKey, TValue>();
            foreach (var pair in obj) {
                TKey key;
                if (pair.Key == NULL_KEY) {
                    key = default;
                } else {
                    key = (TKey)Convert.ChangeType(pair.Key, typeof(TKey));
                }

                TValue value = pair.Value.FromPolymorphic<TValue>();
                dict.Add(key, value);
                onCreate?.Invoke(key, value);
            }
            return dict;
        }
    }
}
