using Editor;
using Sandbox;

namespace MANIFOLD.AnimGraph.Editor {
    public class StatusBar : Widget {
        public class AssetButton : Widget {
            public Asset asset;
            
            public AssetButton(Widget parent) : base(parent) {
                Layout = Layout.Column();
                FixedSize = new Vector2(250, 48);
            }

            protected override void OnPaint() {
                Paint.ClearPen();
                Paint.SetBrushLinear(new Vector2(0, Size.y * -0.2f), Size.WithX(0), Theme.ControlBackground, Color.Parse(ModuleData.BG_COLOR).Value.Darken(0.5f));
                Paint.DrawRect(LocalRect, Theme.ControlRadius);

                if (asset == null) return;
                
                var thumbnail = asset.GetAssetThumb();
                var thumbnailRect = LocalRect.Shrink(4);
                thumbnailRect.Width = thumbnailRect.Height;
                
                Paint.Draw(thumbnailRect, thumbnail);

                var textRect = LocalRect;
                textRect.Left = thumbnailRect.Right + 4;
                textRect = textRect.Shrink(4);

                Paint.TextAntialiasing = true;
                
                Paint.SetHeadingFont(12f, 600);
                var nameSize = Paint.MeasureText(asset.Name);
                
                Paint.SetDefaultFont();
                var pathSize = Paint.MeasureText(asset.Path);
                
                float totalHeight = nameSize.y + pathSize.y;
                
                textRect = textRect.Shrink(0, (textRect.Height - totalHeight) * 0.5f);
                
                var activeRect = textRect;
                activeRect.Height = nameSize.y;
                Paint.SetHeadingFont(12f, 600);
                Paint.SetPen(Theme.Text);
                Paint.DrawText(activeRect, asset.Name, TextFlag.LeftTop);
                
                activeRect = textRect;
                activeRect.Top += nameSize.y;
                Paint.SetDefaultFont();
                Paint.SetPen(Theme.TextDisabled);
                Paint.DrawText(activeRect, asset.Path, TextFlag.LeftTop);
            }
        }

        private Asset asset;
        private AnimGraph graph;

        private AssetButton assetButton;

        public Asset Asset {
            get => asset;
            set {
                asset = value;
                assetButton.asset = asset;
            }
        }
        
        public AnimGraph Graph {
            get => graph;
            set {
                graph = value;
            }
        }
        
        public StatusBar(Widget parent) : base(parent) {
            Name = "StatusBar";

            Layout = Layout.Row();
            Layout.Margin = 4;
            Layout.Spacing = 8;

            assetButton = Layout.Add(new AssetButton(this));
            Layout.Add(new IconButton("save"));
            
            Layout.AddStretchCell();
        }

        protected override void OnPaint() {
            base.OnPaint();
            Paint.SetBrushAndPen(Theme.SurfaceBackground);
            Paint.DrawRect(new Rect(0, Size), Theme.ControlRadius);
        }
    }
}
