using System;
using System.Collections.Generic;
using System.Linq;
using Editor;
using Sandbox;

namespace MANIFOLD.AnimGraph.Editor {
    public class ParameterTable : Widget {
        private record CreateOption(TypeDescription Type, string Name, Color Color);

        private readonly AnimGraphEditor editor;

        private Widget canvas;
        private Dictionary<Parameter, ParameterWidget> widgets;
        
        public ParameterTable(AnimGraphEditor editor) : base(editor) {
            this.editor = editor;
            editor.OnGraphReload += RebuildList;
            
            Name = "ParameterList";
            WindowTitle = "Parameters";

            Layout = Layout.Column();
            Layout.Margin = 2;
            Layout.Spacing = 4;

            var row = Layout.AddRow();
            var addButton = row.Add(new Button("Add", "add"));
            addButton.Clicked = ShowAddMenu;
            row.AddStretchCell();

            var scroll = new ScrollArea(this);
            canvas = scroll.Canvas = new Widget(scroll);
            canvas.Layout = Layout.Column();
            canvas.SetSizeMode(SizeMode.Default, SizeMode.Flexible);
            
            Layout.Add(scroll);
            
            widgets = new Dictionary<Parameter, ParameterWidget>();
        }

        public void RebuildList() {
            canvas.Layout.Clear(true);
            widgets.Clear();

            foreach (var param in editor.GraphResource.Parameters.Values) {
                var widget = new ParameterWidget(canvas);
                widget.Parameter = param;
                widget.OnSelected += ParameterSelectCallback;
                canvas.Layout.Add(widget);
                widgets.Add(param, widget);
            }
            canvas.Layout.AddStretchCell();
        }

        public void OnSelectionChanged(Parameter previous, Parameter current) {
            foreach (var widget in widgets.Values) {
                widget.Selected = false;
                widget.Update();
            }
            if (current != null) {
                widgets[current].Selected = true;
                widgets[current].Update();
            }
        }
        
        private void ParameterSelectCallback(ParameterWidget widget) {
            editor.SelectedParameter = widget.Parameter;
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
            editor.GraphResource.Parameters.Add(instance.ID, instance);
            RebuildList();
        }
    }
}
