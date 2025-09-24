using System;
using System.Collections.Generic;
using System.Linq;
using Editor;
using Sandbox;

namespace MANIFOLD.AnimGraph.Editor {
    public class ParameterPanel : Widget {
        private record CreateOption(TypeDescription Type, string Name, Color Color);

        private readonly AnimGraphEditor editor;
        private readonly ScrollArea scroll;
        private Widget scrollCanvas;
        private readonly ParameterTable table;
        
        public ParameterPanel(AnimGraphEditor editor) : base(editor) {
            this.editor = editor;
            
            Name = "ParameterPanel";
            WindowTitle = "Parameters";
            
            Layout = Layout.Column();
            Layout.Margin = 2;
            Layout.Spacing = 4;

            var row = Layout.AddRow();
            var addButton = row.Add(new Button("Add", "add"));
            addButton.Clicked = ShowAddMenu;
            row.AddStretchCell();

            scroll = new ScrollArea(this);
            table = new ParameterTable(editor);
            
            scrollCanvas = new Widget(scroll);
            scrollCanvas.Layout = Layout.Column();
            scrollCanvas.Layout.Add(table);
            scrollCanvas.Layout.AddStretchCell();
            
            scroll.Canvas = scrollCanvas;
            Layout.Add(scroll);
        }
        
        private void ShowAddMenu() {
            ContextMenu menu = new ContextMenu(this);

            List<CreateOption> options = new List<CreateOption>();
            
            var baseType = EditorTypeLibrary.GetType(typeof(Parameter<>));
            var types = EditorTypeLibrary.GetTypes(typeof(Parameter));
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
        }
    }
}
