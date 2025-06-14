using System;
using System.Collections.Generic;
using System.Linq;
using Editor;
using Sandbox;

namespace MANIFOLD.AnimGraph.Editor {
    public class ParameterTable : Widget {
        private record CreateOption(TypeDescription Type, string Name, Color Color);
        
        private AnimGraph graph;

        private Widget parameterArea;
        
        public AnimGraph Graph {
            get => graph;
            set {
                graph = value;
                Refresh();
            }
        }
        
        public ParameterTable() {
            Name = "ParameterList";
            WindowTitle = "Parameters";

            Layout = Layout.Column();
            Layout.Margin = 2;
            Layout.Spacing = 4;

            var bar = new Widget(this);
            bar.Layout = Layout.Row();
            
            var addButton = new Button("Add", "add", bar);
            addButton.Clicked = ShowAddMenu;
            bar.Layout.Add(addButton);
            bar.Layout.AddStretchCell();

            Layout.Add(bar);

            var scroll = new ScrollArea(this);
            parameterArea = scroll.Canvas = new Widget(scroll);
            parameterArea.Layout = Layout.Column();
            parameterArea.SetSizeMode(SizeMode.Flexible, SizeMode.CanGrow);
            
            Layout.Add(scroll);
        }

        public void Refresh() {
            parameterArea.Layout.Clear(true);

            foreach (var param in graph.Parameters.Values) {
                var widget = new ParameterWidget(parameterArea);
                widget.Parameter = param;
                parameterArea.Layout.Add(widget);
            }
            parameterArea.Layout.AddStretchCell();
        }

        private void ShowAddMenu() {
            ContextMenu menu = new ContextMenu(this);

            List<CreateOption> options = new List<CreateOption>();
            
            var baseType = EditorTypeLibrary.GetType(typeof(Parameter<>));
            var types = EditorTypeLibrary.GetTypes(typeof(Parameter));
            Log.Info($"Type count: {types.Count()}");
            foreach (var type in types) {
                var attr = type.GetAttribute<ExposeToAnimGraphAttribute>();
                if (attr == null) continue;
                if (type.BaseType != baseType) continue;
                
                var arg = type.TargetType.BaseType.GenericTypeArguments[0];
                var name = type.GetAttribute<TitleAttribute>()?.Value ?? arg.Name;
                
                options.Add(new CreateOption(type, name, attr.Color));
            }

            for (int i = 0; i < options.Count; i++) {
                var option = options[i];
                var widget = menu.AddOption($"{option.Name} Parameter", null, () => AddParameter(option.Type));

                var tempIcon = new Pixmap(16);
                using (Paint.ToPixmap(tempIcon)) {
                    Paint.SetBrushAndPen(option.Color);
                    Paint.DrawRect(new Rect(4, 0, 6, 16));
                }
                widget.SetIcon(tempIcon);
            }
            
            menu.OpenAtCursor();
        }

        private void AddParameter(TypeDescription type) {
            var instance = (Parameter)Activator.CreateInstance(type.TargetType); // TypeDescription has a create function but it doesnt feel like working
            graph.Parameters.Add(instance.ID, instance);
            Refresh();
        }
    }
}
