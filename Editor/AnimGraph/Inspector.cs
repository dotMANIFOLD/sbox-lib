using System;
using System.Collections.Generic;
using System.Linq;
using Editor;
using Sandbox;

namespace MANIFOLD.AnimGraph.Editor {
    public class Inspector : Widget {
        public const string EMPTY_LABEL = "Select a node to view it's properties...";

        private SerializedObject serialized;

        public event Action OnChange;
        
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
            serialized.OnPropertyChanged += OnPropertyChanged;
            
            var sheet = new ControlSheet();
            sheet.AddObject(serialized);
            
            Layout.Add(sheet);
            Layout.AddStretchCell();
        }
        
        private void ShowLabel(string text) {
            var label = new Label(text);
            label.Color = Color.Gray.WithAlpha(0.5f);
            
            Layout.Add(label);
            Layout.AddStretchCell();
        }

        private void OnPropertyChanged(SerializedProperty property) {
            OnChange?.Invoke();
        }
    }
}
