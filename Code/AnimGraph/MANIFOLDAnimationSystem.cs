using System;
using System.Linq;
using Sandbox;
using Sandbox.Diagnostics;
using Sandbox.Utility;

namespace MANIFOLD.AnimGraph {
    public class MANIFOLDAnimationSystem : GameObjectSystem<MANIFOLDAnimationSystem> {
        public MANIFOLDAnimationSystem(Scene scene) : base(scene) {
            Listen(Stage.UpdateBones, 0, UpdateAnimation, "MANIFOLD_UpdateAnimation");
        }

        private void UpdateAnimation() {
            if (Scene.IsEditor) return;
            
            var animators = Scene.GetAll<MANIFOLDAnimator>()
                .Where(x => x.Enabled && x.AutoUpdate && x.IsPlaying)
                .ToArray();

            foreach (var animator in animators) {
                // Cache persistent data before threaded updates
                _ = ModelPersistentData.Get(animator.Renderer.Model);
            }
            
            try {
                Parallel.ForEach(animators, ProcessAnimator);
            } catch (Exception e) {
                Log.Error($"Error while updating animation: {e}");
            }
        }

        private void ProcessAnimator(MANIFOLDAnimator animator) {
            animator.Update(Time.Delta);
        }
    }
}
