using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Nodes;
using Sandbox;

namespace MANIFOLD.Utility {
    public static class PolymorphicExtensions {
        public const string PRIMITIVE_FIELD = "__prim";
        public const string VALUE_FIELD = "__value";
        public const string TYPE_FIELD = "__type";
        public const string TYPE_ELEMENT_FIELD = "__elements";
        public const string NULL_KEY = "__null";

        public static JsonNode ToPolymorphic<T>(this T obj) {
            var type = obj.GetType();
            var typeDef = TypeLibrary.GetType(type);

            JsonNode node = Json.ToNode(obj);
            if (IsPrimitive(type)) {
                JsonObject jsonObj = new JsonObject();
                jsonObj[PRIMITIVE_FIELD] = Json.ToNode(true);
                jsonObj[VALUE_FIELD] = node;
                node = jsonObj;
            }
            
            if (typeDef.IsGenericType) {
                node[TYPE_FIELD] = Json.ToNode(typeDef.TargetType, typeof(Type));
                node[TYPE_ELEMENT_FIELD] = Json.ToNode(typeDef.GenericArguments);
            } else {
                node[TYPE_FIELD] = Json.ToNode(type, typeof(Type));
            }
            
            return node;
        }

        public static T FromPolymorphic<T>(this JsonNode node) {
            var type = Json.FromNode<Type>(node[TYPE_FIELD]);
            var typeDef = TypeLibrary.GetType(type);
            
            var primNode = node[PRIMITIVE_FIELD];
            if (primNode != null && primNode.GetValue<bool>()) {
                return (T)Json.FromNode(node[VALUE_FIELD], type);
            }
            
            if (typeDef.IsGenericType) {
                var elements = Json.FromNode<Type[]>(node[TYPE_ELEMENT_FIELD]);
                type = typeDef.MakeGenericType(elements);
            }
            
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

        public static Dictionary<TKey, TValue> DeserializePolymorphic<TKey, TValue>(this JsonObject obj, Action<TKey, TValue> onCreate = null) where TKey : IParsable<TKey> {
            Dictionary<TKey, TValue> dict = new Dictionary<TKey, TValue>();
            foreach (var pair in obj) {
                TKey key;
                if (pair.Key == NULL_KEY) {
                    key = default;
                } else {
                    key = TKey.Parse(pair.Key, null);
                }

                TValue value = pair.Value.FromPolymorphic<TValue>();
                dict.Add(key, value);
                onCreate?.Invoke(key, value);
            }
            return dict;
        }

        public static bool IsPrimitive(Type type) {
            if (type == typeof(bool)) return true;
            if (type == typeof(byte)) return true;
            if (type == typeof(sbyte)) return true;
            if (type == typeof(short)) return true;
            if (type == typeof(ushort)) return true;
            if (type == typeof(int)) return true;
            if (type == typeof(uint)) return true;
            if (type == typeof(long)) return true;
            if (type == typeof(ulong)) return true;
            if (type == typeof(float)) return true;
            if (type == typeof(double)) return true;
            if (type == typeof(decimal)) return true;
            if (type == typeof(string)) return true;
            if (type == typeof(Vector2)) return true;
            if (type == typeof(Vector3)) return true;
            if (type == typeof(Vector4)) return true;
            if (type == typeof(Rotation)) return true;
            if (type == typeof(Angles)) return true;
            return false;
        }
    }
}
