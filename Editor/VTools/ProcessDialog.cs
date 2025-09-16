using System;
using System.Collections.Concurrent;
using Editor;
using Sandbox;

namespace MANIFOLD.Editor {
    public class ProcessDialog : Dialog {
        public struct LogEvent {
            public bool error;
            public string message;
        }
        
        private class LogWidget : Widget {
            private readonly ScrollArea scrollArea;
            private readonly Widget logCanvas;
            
            public LogWidget(Widget widget) : base(widget) {
                Layout = Layout.Column();
                Layout.Margin = 4;
                
                SetSizeMode(SizeMode.Default, SizeMode.Default);
                
                scrollArea = Layout.Add(new ScrollArea(this));
                logCanvas = scrollArea.Canvas = new Widget();
                logCanvas.SetSizeMode(SizeMode.Default, SizeMode.Expand);
                logCanvas.TranslucentBackground = true;
                logCanvas.Layout = Layout.Column(true);
                logCanvas.Layout.Margin = 2;
            }
            
            protected override void OnPaint() {
                Paint.SetBrushAndPen(Theme.BaseAlt);
                Paint.DrawRect(LocalRect);
            }

            public void AddInfo(string message) {
                logCanvas.Layout.Add(new Label(message, logCanvas));
                scrollArea.VerticalScrollbar.Value = scrollArea.VerticalScrollbar.Maximum;
            }

            public void AddError(string message) {
                logCanvas.Layout.Add(new Label(message, logCanvas) { Color = Theme.Red });
                scrollArea.VerticalScrollbar.Value = scrollArea.VerticalScrollbar.Maximum;
            }
        }

        private class ProgressBar : Widget {
            public float progress;
            public bool finished;
            public bool failed;

            public ProgressBar(Widget widget) : base(widget) {
                FixedHeight = 28;
            }
            
            protected override void OnPaint() {
                Rect drawRect = LocalRect;
                
                Paint.SetBrushAndPen(Theme.BaseAlt);
                Paint.DrawRect(drawRect);

                drawRect = drawRect.Shrink(2);
                Paint.SetBrushAndPen(Theme.ControlBackground);
                Paint.DrawRect(drawRect);

                drawRect.Right *= progress;
                Color color;
                if (failed) color = Theme.Red;
                else if (finished) color = Theme.Green;
                else color = Theme.Blue;
                
                Paint.SetBrushAndPen(color);
                Paint.DrawRect(drawRect);
            }
        }
        
        private readonly LogWidget logs;
        private readonly ProgressBar bar;
        private readonly Layout buttonBar;
        private readonly ConcurrentStack<LogEvent> eventQueue;
        
        public Action onCancel;
        
        public ProcessDialog() : base(null) {
            Window.Title = "Process Watch";
            Window.Size = new Vector2(400, 300);
            
            Layout = Layout.Column();
            Layout.Margin = 16;
            Layout.Spacing = 8;

            var label = Layout.Add(new Label("Running command..."));
            label.SetSizeMode(SizeMode.Default, SizeMode.CanShrink);
            
            logs = Layout.Add(new LogWidget(this));
            bar = Layout.Add(new ProgressBar(this));
            bar.progress = 0.5f;
            
            buttonBar = Layout.AddRow();
            buttonBar.AddStretchCell();
            buttonBar.Add(new Button("Cancel") { Clicked = () => { onCancel?.Invoke(); } });
            
            eventQueue = new ConcurrentStack<LogEvent>();
        }

        [EditorEvent.Frame]
        private void Update() {
            while (eventQueue.Count > 0) {
                var success = eventQueue.TryPop(out var evt);
                if (!success) break;
                
                if (evt.error) logs.AddError(evt.message);
                else logs.AddInfo(evt.message);
            }
        }

        public void AddInfo(string message) {
            eventQueue.Push(new LogEvent() {
                message = message,
            });
        }

        public void AddError(string message) {
            eventQueue.Push(new LogEvent() {
                error = true,
                message = message,
            });
        }

        public void SetProgress(float progress) {
            bar.progress = progress;
        }

        public void Finished() {
            bar.progress = 1;
            bar.finished = true;
            ShowCloseButton();
        }

        public void Failed() {
            bar.progress = 1;
            bar.failed = true;
            ShowCloseButton();
        }
        
        private void ShowCloseButton() {
            buttonBar.Clear(true);
            
            buttonBar.AddStretchCell();
            buttonBar.Add(new Button.Primary("Close") { Clicked = () => Close() });
        }
    }
}
