using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Editor;
using Sandbox;

namespace MANIFOLD.AnimGraph.Editor {
    public class Inspector : Widget {
        public const string EMPTY_LABEL = "Select a node to view it's properties...";

        private SerializedObject serialized;
        private bool changingCollection;
        private int oldCollectionCount;
        private bool addOperation;
        private int addCount;

        public event Action OnInputChanged;
        
        public Inspector() {
            Name = "Inspector";
            WindowTitle = "Inspector";

            Layout = Layout.Column();
            Layout.Margin = 2;
            ShowLabel(EMPTY_LABEL);
        }

        public void SetNodes(IEnumerable<GraphNode> nodes) {
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
            
            serialized = EditorTypeLibrary.GetSerializedObject(nodes.First().RealNode);
            serialized.OnPropertyPreChange += OnPropertyPreChange;
            serialized.OnPropertyChanged += OnPropertyChanged;
            
            var sheet = new ControlSheet();
            sheet.AddObject(serialized);
            
            var scroll = new ScrollArea(this);
            scroll.Canvas = new Widget();
            scroll.Canvas.Layout = Layout.Column();
            scroll.Canvas.SetSizeMode(SizeMode.Flexible, SizeMode.CanGrow);
            
            scroll.Canvas.Layout.Add(sheet);
            scroll.Canvas.Layout.AddStretchCell();
            
            Layout.Add(scroll);
        }
        
        private void ShowLabel(string text) {
            var label = new Label(text);
            label.Color = Color.Gray.WithAlpha(0.5f);
            
            Layout.Add(label);
            Layout.AddStretchCell();
        }

        private void OnPropertyPreChange(SerializedProperty property) {
            // this check apparently has inheritance, so all elements of an array count as having this attribute
            // just maybe it should be changed
            if (!property.HasAttribute<InputAttribute>()) return;
            
            Type type = property.PropertyType;
            bool validType = type.IsAssignableTo(typeof(IEnumerable<NodeReference>)) || type.IsAssignableTo(typeof(IEnumerable<INodeReferenceProvider>));
            if (!validType) return;
            
            changingCollection = true;
            oldCollectionCount = property.GetValue<IEnumerable<object>>().Count();
        }
        
        private void OnPropertyChanged(SerializedProperty property) {
            if (addCount > 0) {
                addCount--;

                if (addCount == 0) {
                    OnInputChanged?.Invoke();
                }
            }
            
            if (changingCollection) {
                var enumerable = property.GetValue<IEnumerable<object>>();
                int newCount = enumerable.Count();

                addCount = newCount - oldCollectionCount;
                addOperation = addCount > 0;

                if (!addOperation) {
                    OnInputChanged?.Invoke();
                }

                changingCollection = false;
            }
        }
    }
}
