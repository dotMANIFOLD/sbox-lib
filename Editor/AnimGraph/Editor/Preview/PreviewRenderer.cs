using Editor;
using Sandbox;

namespace MANIFOLD.AnimGraph.Editor {
    public class PreviewRenderer : SceneRenderingWidget {
        private AnimGraph graph;
        
        private MANIFOLDAnimator animator;
        private SkinnedModelRenderer model;
        private CameraComponent camera;

        public AnimGraph Graph {
            get => graph;
            set {
                graph = value;
                animator.Animations = graph.Collection;
                model.Model = graph.Collection.Model;
            }
        }
        
        public PreviewRenderer(Widget parent) : base(parent) {
            Scene = Scene.CreateEditorScene();
            using (Scene.Push()) {
                camera = new GameObject(true, "cam").AddComponent<CameraComponent>();
                camera.BackgroundColor = Color.Gray.Darken(0.7f);
                camera.ZNear = 3f;
                camera.WorldRotation = Rotation.From(15f, -135f, 0f);
                camera.WorldPosition = new Vector3(100f, 100f, 70f) * 0.6f;
                
                var light = new GameObject(true, "light").AddComponent<DirectionalLight>();
                light.WorldRotation = Rotation.From(60, 120, 0);
                light.SkyColor = Color.White.Darken(0.8f);

                model = new GameObject(true, "model").AddComponent<SkinnedModelRenderer>();
                animator = model.GameObject.AddComponent<MANIFOLDAnimator>();
                animator.Renderer = model;
            }
        }

        public override void PreFrame() {
            UpdateCamera();
            base.PreFrame();
        }

        private void UpdateCamera() {
            Gizmo.Draw.Grid(Gizmo.GridAxis.XY, 10);

            Scene.Camera.UpdateSceneCamera(Gizmo.Camera);
            SceneEditorExtensions.FirstPersonCamera(GizmoInstance, Gizmo.Camera, Parent, true);
            camera.WorldPosition = Gizmo.Camera.Position;
            camera.WorldRotation = Gizmo.Camera.Rotation;
        }
    }
}
