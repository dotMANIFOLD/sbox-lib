using Editor;
using Sandbox;
using Application = Editor.Application;

namespace MANIFOLD.AnimGraph.Editor {
    public class PreviewRenderer : SceneRenderingWidget {
        private AnimGraph graph;
        
        private MANIFOLDAnimator animator;
        private SkinnedModelRenderer model;
        private CameraComponent camera;

        private float time;
        private float cameraDistance;
        private bool playing;
        private bool paused;
        
        public AnimGraph Graph {
            get => graph;
            set {
                graph = value;
                animator.AnimGraph = value;
                animator.Animations = graph.Resources;
                model.Model = graph.Resources.Model;
                ResetCamera();
            }
        }

        public float Time => time;
        public float TimeScale { get; set; } = 1f;
        public bool IsPlaying => playing;
        public bool Paused => paused;
        
        public PreviewRenderer(Widget parent) : base(parent) {
            SetSizeMode(SizeMode.Default, SizeMode.Flexible);
            
            Scene = Scene.CreateEditorScene();
            using (Scene.Push()) {
                camera = new GameObject(true, "cam").AddComponent<CameraComponent>();
                camera.BackgroundColor = Color.Gray.Darken(0.7f);
                camera.ZNear = 3f;
                
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
            Scene.EditorTick(time, RealTime.Delta * TimeScale);
            if (animator.IsPlaying) {
                time += RealTime.Delta * TimeScale;
            }
            base.PreFrame();
        }
        
        public void Play() {
            animator.RebuildParameters();
            animator.RebuildGraph();
            animator.Play();
            time = 0;
            playing = true;
            paused = false;
        }

        public void Pause() {
            if (paused) {
                animator.Play();
            } else {
                animator.Stop();
            }
            paused = !paused;
        }

        public void Stop() {
            animator.Stop();
            model.SceneModel.ClearBoneOverrides();
            time = 0;
            playing = false;
            paused = false;
        }

        private void ResetCamera() {
            var dir = new Vector3(0.7f, 0.7f, 0.5f);
            cameraDistance = MathX.SphereCameraDistance(model.Bounds.Size.Length * 0.5f, camera.FieldOfView) * 0.6f;
            var aspect = Size.x / Size.y;
            if (aspect > 1) cameraDistance *= aspect;
            
            camera.WorldPosition = model.Bounds.Center + dir * cameraDistance;
            camera.WorldRotation = Rotation.LookAt(dir * -1);
        }
        
        private void UpdateCamera() {
            Gizmo.Draw.Grid(Gizmo.GridAxis.XY, 10);

            Scene.Camera.UpdateSceneCamera(Gizmo.Camera);
            GizmoInstance.FirstPersonCamera(Gizmo.Camera, Parent, true);
            camera.WorldPosition = Gizmo.Camera.Position;
            camera.WorldRotation = Gizmo.Camera.Rotation;
        }
    }
}
