using System;
using System.Collections.Generic;
using System.Linq;
using Editor;
using Editor.NodeEditor;
using MANIFOLD.Jobs;
using Sandbox;

namespace MANIFOLD.JobGraph.Editor {
    public abstract class GraphNode : INode {
        protected JobGraphWrapper wrapper;
        protected NodeUI ui;
        protected Vector2 position;

        public JobGraphWrapper Wrapper => wrapper;
        public NodeUI UI => ui;
        
        public abstract string Identifier { get; }
        public abstract DisplayInfo DisplayInfo { get; }

        public Vector2 Position {
            get => position;
            set {
                position = value;
                if (ui != null) ui.Position = value;
            }
        }
        public bool AutoSize => false;
        public Vector2 ExpandSize { get; set; }

        public abstract IEnumerable<IPlugIn> Inputs { get; }
        public abstract IEnumerable<IPlugOut> Outputs { get; }

        public bool HasTitleBar => true;
        public bool IsReachable => true;
        public bool CanClone => false;
        public bool CanRemove => false;

        public Pixmap Thumbnail => null;

        public virtual string ErrorMessage => null;
        
        public event Action Changed;

        public GraphNode(JobGraphWrapper wrapper) {
            this.wrapper = wrapper;
        }

        public void Update(int zIndex) {
            ui.ZIndex = zIndex;
            ui.MarkNodeChanged();
        }
        
        public void OnPaint(Rect rect) {
            
        }
        
        public void OnDoubleClick(MouseEvent e) {
            
        }

        public abstract NodeUI CreateUI(GraphView view);

        public Color GetPrimaryColor(GraphView view) {
            return Color.Gray;
        }

        public Menu CreateContextMenu(NodeUI node) {
            return null;
        }
    }
}
