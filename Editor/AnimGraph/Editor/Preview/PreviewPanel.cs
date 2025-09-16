using Editor;
using Sandbox;
using Sandbox.UI;
using Label = Editor.Label;

namespace MANIFOLD.AnimGraph.Editor {
    public class PreviewPanel : Widget {
        private readonly AnimGraphEditor editor;
        
        private PreviewRenderer renderer;
        private IconButton stateButton;
        private IconButton pauseButton;
        
        public PreviewPanel(AnimGraphEditor editor) : base(editor) {
            this.editor = editor;
            editor.OnGraphReload += OnGraphReload;
            
            Name = "Preview";
            WindowTitle = "Preview";

            Layout = Layout.Column();
            Layout.Add(renderer = new PreviewRenderer(this));

            var row = Layout.AddRow();
            row.Margin = new Margin(16, 4);
            row.Spacing = 4;
            stateButton = row.Add(new IconButton("play_arrow", OnToggleState));
            pauseButton = row.Add(new IconButton("pause", OnPause));
            pauseButton.ToolTip = "Pause";
            
            row.AddStretchCell();
            var timeLabel = row.Add(new Label("0.00"));
            timeLabel.MinimumWidth = 40;
            timeLabel.Alignment = TextFlag.Center;
            timeLabel.Bind("Text").ReadOnly().From(() => renderer.Time.ToString("N2"), null);
            
            row.AddStretchCell();
            row.Add(new Label("Time Scale"));
            var timeScaleSlider = row.Add(new FloatSlider(this) { Minimum = 0f, Maximum = 2f, Value = 1f });
            timeScaleSlider.Bind("Value").From(renderer, nameof(PreviewRenderer.TimeScale));
            timeScaleSlider.FixedWidth = 100;
            
            var timeScaleLabel = row.Add(new Label("1.00"));
            timeScaleLabel.FixedWidth = 28;
            timeScaleLabel.Alignment = TextFlag.Center;
            timeScaleLabel.Bind("Text").ReadOnly().From(() => renderer.TimeScale.ToString("N2"), null);

            var resetButton = row.Add(new IconButton("refresh"));
            resetButton.ToolTip = "Reset time scale";
            
            UpdateStateButton();
            UpdatePauseButton();
        }
        
        private void OnToggleState() {
            if (renderer.IsPlaying) renderer.Stop();
            else renderer.Play();
            UpdateStateButton();
            UpdatePauseButton();    
        }

        private void OnPause() {
            renderer.Pause();
            UpdatePauseButton();
        }
        
        private void UpdateStateButton() {
            stateButton.Icon = renderer.IsPlaying ? "stop" : "play_arrow";
            stateButton.Foreground = renderer.IsPlaying ? Theme.Red : Theme.Green;
            stateButton.ToolTip = renderer.IsPlaying ? "Stop" : "Play";
        }

        private void UpdatePauseButton() {
            pauseButton.Enabled = renderer.IsPlaying;
            pauseButton.Foreground = renderer.Paused ? Theme.Blue : Theme.TextButton;
        }
        
        private void OnGraphReload() {
            renderer.Graph = editor.GraphResource;
        }
    }
}
