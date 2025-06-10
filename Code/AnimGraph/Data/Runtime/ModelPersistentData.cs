using System.Collections.Generic;
using Sandbox;

namespace MANIFOLD.AnimGraph {
    public static class ModelPersistentData {
        public class Data {
            public Dictionary<string, int> remapTable;
            public Pose bindPose;
        }

        private static Dictionary<Model, Data> dictionary = new();

        public static Data Create(Model model) {
            Dictionary<string, int> remap = new Dictionary<string, int>(model.BoneCount);
            Transform[] bindPose = new Transform[model.BoneCount];
            for (int i = 0; i < model.BoneCount; ++i) {
                var bone = model.Bones.AllBones[i];
                remap.Add(bone.Name, i);
                bindPose[i] = bone.Parent?.LocalTransform.ToLocal(bone.LocalTransform) ?? Transform.Zero;
            }

            var data = new Data() {
                remapTable = remap,
                bindPose = new Pose(bindPose, remap)
            };
            dictionary[model] = data;
            return data;
        }
        
        public static Data Get(Model model) {
            if (dictionary.TryGetValue(model, out Data data)) {
                return data;
            }
            return Create(model);
        }
    }
}
