using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Editor;
using Sandbox;

namespace MANIFOLD.AnimGraph.Editor {
    public class InspectorPanel : Widget {
        public const string EVENT_PREFIX = $"{AnimGraphEditor.EVENT_PREFIX}.inspector";
        public const string EVENT_REBUILD = $"{EVENT_PREFIX}.rebuild";
        
        public const string EMPTY_LABEL = "Select a node to view it's properties...";

        private readonly AnimGraphEditor editor;
        private readonly ScrollArea scrollArea;
        private readonly ControlSheet sheet;
        private readonly Label label;
        
        private SerializedObject serialized;
        private bool changingCollection;
        private int oldCollectionCount;
        private bool addOperation;
        private int addCount;
        
        public event Action OnNodeInputChanged;
        
        public InspectorPanel(AnimGraphEditor editor) : base(editor) {
            this.editor = editor;
            
            Name = "Inspector";
            WindowTitle = "Inspector";

            Layout = Layout.Column();
            Layout.Margin = 2;
            ShowLabel(EMPTY_LABEL);
        }

        public void SetNodes(IEnumerable<BaseNode> nodes) {
            Layout.Clear(true);
            
            if (nodes == null) {
                ShowLabel(EMPTY_LABEL);
                return;
            }
            
            int count = nodes.Count();
            if (count == 0) {
                ShowLabel(EMPTY_LABEL);
                return;
            } else if (count > 1) {
                ShowLabel("Multi-edit not supported");
                return;
            }
            
            serialized = nodes.First().GetSerialized();
            serialized.OnPropertyPreChange += OnPropertyPreChange;
            serialized.OnPropertyChanged += OnPropertyChanged;
            
            var sheet = new ControlSheet();
            sheet.AddObject(serialized, (prop) => {
                bool isDebug = prop.PropertyType.IsAssignableTo(typeof(NodeRef)) || prop.Name == nameof(BaseNode.ID);
                return (!isDebug || editor.ShowDebugInfo) && DefaultSheetFilter(prop);
            });
            ShowSheet(sheet);
        }

        public void SetParameter(Parameter parameter) {
            Layout.Clear(true);

            if (parameter == null) {
                ShowLabel(EMPTY_LABEL);
                return;
            }
            
            serialized = EditorTypeLibrary.GetSerializedObject(parameter);

            var sheet = new ControlSheet();
            sheet.AddObject(serialized, (prop) => {
                return DefaultSheetFilter(prop) && prop.Name != "Value" && prop.Name != "backingField";
            });
            ShowSheet(sheet);
        }

        private void ShowSheet(ControlSheet sheet) {
            var scroll = new ScrollArea(this);
            scroll.Canvas = new Widget();
            scroll.Canvas.Layout = Layout.Column();
            scroll.Canvas.SetSizeMode(SizeMode.Default, SizeMode.Flexible);
            
            scroll.Canvas.Layout.Add(sheet);
            scroll.Canvas.Layout.AddStretchCell();
            
            Layout.Add(scroll);
            EditorEvent.Run(EVENT_REBUILD);
        }
        
        private void ShowLabel(string text) {
            var label = new Label(text);
            label.Color = Color.Gray.WithAlpha(0.5f);
            
            Layout.Add(label);
            Layout.AddStretchCell();
        }

        private bool DefaultSheetFilter(SerializedProperty property) {
            if (property.HasAttribute<HideAttribute>()) return false;
            if (!property.IsPublic) return false;
            return true;
        }
        
        private void OnPropertyPreChange(SerializedProperty property) {
            // this check apparently has inheritance, so all elements of an array count as having this attribute
            // just maybe it should be changed
            if (!property.HasAttribute<InputAttribute>()) return;
            
            Type type = property.PropertyType;
            bool validType = type.IsAssignableTo(typeof(IEnumerable<NodeRef>)) || type.IsAssignableTo(typeof(IEnumerable<INodeRefProvider>));
            if (!validType) return;
            
            changingCollection = true;
            oldCollectionCount = property.GetValue<IEnumerable<object>>()?.Count() ?? 0;
        }
        
        private void OnPropertyChanged(SerializedProperty property) {
            if (addCount > 0) {
                addCount--;

                if (addCount == 0) {
                    OnNodeInputChanged?.Invoke();
                }
            }
            
            if (changingCollection) {
                var enumerable = property.GetValue<IEnumerable<object>>();
                int newCount = enumerable?.Count() ?? 0;

                addCount = newCount - oldCollectionCount;
                addOperation = addCount > 0;

                if (!addOperation) {
                    OnNodeInputChanged?.Invoke();
                }

                changingCollection = false;
            }
            
            editor.GraphResource.StateHasChanged();
        }
    }
}
