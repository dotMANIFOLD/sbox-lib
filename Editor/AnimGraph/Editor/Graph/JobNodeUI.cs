using System;
using System.Linq;
using Editor;
using Editor.NodeEditor;
using Sandbox;

namespace MANIFOLD.AnimGraph.Editor {
    public class JobNodeUI : NodeUI {
        public Color AccentColor { get; set; }
        
        private AnimGraphView betterView;
        private GraphNode betterNode;

        public JobNodeUI(GraphView graph, GraphNode node) : base(graph, node) {
            betterView = (AnimGraphView)graph;
            betterNode = node;
        }

        protected override void OnPaint() {
            var rect = new Rect(0f, Size);
            var radius = 4;

            PrimaryColor = Node.GetPrimaryColor(Graph);
            AccentColor = betterNode.GetAccentColor(Graph);
            if (Paint.HasSelected) {
                PrimaryColor = SelectionOutline;
                AccentColor = AccentColor.Lighten(0.2f);
            }
            else {
                if (!Node.IsReachable) PrimaryColor = PrimaryColor.Desaturate(0.5f).Darken(0.25f);
            }

            if (Node.HasTitleBar) {
                Paint.ClearPen();
                Paint.SetBrush(PrimaryColor.Darken(0.5f));
                Paint.DrawRect(rect, radius);
            } else {
                Paint.ClearPen();
                Paint.SetBrush(PrimaryColor.Darken(0.6f));
                Paint.DrawRect(rect, radius);
            }

            Paint.ClearPen();
            Paint.ClearBrush();

            Paint.ClearPen();
            Paint.SetBrush(PrimaryColor.WithAlpha(0.05f));

            var display = DisplayInfo;

            if (Node.HasTitleBar) {
                // Normal node display, with a title bar and possible thumbnail

                var titleRect = new Rect(rect.Position, new Vector2(rect.Width, TitleHeight)).Shrink(0, 0, 0, 4);
                Paint.SetBrush(AccentColor);
                Paint.ClearPen();
                Paint.DrawRect(titleRect, radius);

                if (display.Icon != null) {
                    Paint.SetPen(PrimaryColor.Lighten(0.7f).WithAlpha(0.7f));
                    Paint.DrawIcon(titleRect.Shrink(4), display.Icon, 17, TextFlag.LeftCenter);
                    titleRect.Left += 18;
                }

                var title = betterNode.GetDisplayName();

                Paint.SetDefaultFont(7, 500);
                Paint.SetPen(PrimaryColor.Lighten(0.8f));
                Paint.DrawText(titleRect.Shrink(5, 0), title, TextFlag.LeftCenter);

                if (Node.Thumbnail is not null || Inputs.Any(x => !x.Inner.InTitleBar) || Outputs.Any(x => !x.Inner.InTitleBar)) {
                    // body inner
                    float borderSize = 3;
                    Paint.ClearPen();
                    Paint.SetBrush(PrimaryColor.Darken(0.6f));
                    Paint.DrawRect(rect.Shrink(borderSize, TitleHeight, borderSize, borderSize), radius - 2);
                }

                if (betterView.editor.NodeTools.ContainsKey(betterNode.RealNode.GetType())) {
                    var iconRect = titleRect;
                    iconRect.Left = iconRect.Width - iconRect.Height;
                    iconRect = iconRect.Shrink(2);
                    
                    Paint.SetPen(PrimaryColor.Lighten(0.8f));
                    Paint.ClearBrush();
                    Paint.DrawIcon(iconRect, "launch", iconRect.Height);
                }
            } else if (Node.DisplayInfo.Icon is { } icon) {
                // Node is an icon without text, e.g. for operators

                var scale = icon.Length == 2 && !char.IsLetterOrDigit(icon[0]) ? 0.5f : icon == "|" ? 0.75f : 1f;

                Paint.SetPen(Theme.TextControl);
               //  Paint.DrawIcon(ThumbRe, icon, (Math.Min(_thumbRect.Width, _thumbRect.Height) - 8f) * scale);
            }

            Node.OnPaint(rect);

            if (Paint.HasSelected) {
                Paint.SetPen(SelectionOutline, 2.0f);
                Paint.ClearBrush();
                Paint.DrawRect(rect, radius);
            } else if (Paint.HasMouseOver) {
                Paint.SetPen(SelectionOutline, 1.0f);
                Paint.ClearBrush();
                Paint.DrawRect(rect, radius);
            }
        }
    }
}
