using System.Collections.Generic;
using System.Linq;
using Editor;
using MANIFOLD.Animation;
using Sandbox;

namespace MANIFOLD.AnimGraph.Editor {
    public abstract class ResourceRefControl<TRef, TData> : ControlWidget where TRef : ResourceRef<TData> where TData : GameResource, INamedResource {
        private readonly IconButton typeButton;
        private readonly Layout body;

        private AnimGraph graph;
        private TRef reference;
        private SerializedObject serializedObject;

        public AnimGraph Graph => graph;
        public TRef Reference => reference;
        
        public ResourceRefControl(SerializedProperty property) : base(property) {
            HorizontalSizeMode = SizeMode.CanGrow | SizeMode.Expand;
            MouseTracking = true;
            AcceptDrops = true;
            IsDraggable = true;

            Layout = Layout.Row();
            Layout.Spacing = 2;

            body = Layout.AddRow();

            typeButton = Layout.Add(new IconButton.WithCornerIcon("list_alt") {
                OnClick = ShowMenu,
                Background = Color.Transparent,
                IconSize = 16,
                CornerIconSize = 16,
                CornerIconOffset = 2,
                ToolTip = "Source Type"
            });
            
            reference = property.GetValue<TRef>();
            property.TryGetAsObject(out serializedObject);
            RebuildControl();
        }

        protected override void PaintUnder() {
            
        }

        protected abstract IEnumerable<string> GetAvailableNames();
        
        
        [Event(AnimGraphEditor.EVENT_REBUILD)]
        private void RebuildControl() {
            body.Clear(true);

            graph = GetContext<AnimGraph>(AnimGraphEditor.CONTEXT_GRAPH);
            
            switch (reference.Mode) {
                case ResourceRef.RefMode.Named: {
                    var property = serializedObject.GetProperty(nameof(ResourceRef.NamedReference));

                    if (graph != null) {
                        var comboBox = body.Add(new ComboBox());
                    
                        comboBox.AddItem("<None>", selected: reference.NamedReference == null, onSelected: () => {
                            property.SetValue((string)null);
                        });
                        foreach (var name in GetAvailableNames()) {
                            comboBox.AddItem(name, selected: reference.NamedReference == name, onSelected: () => {
                                property.SetValue(name);
                            });
                        }
                    } else {
                        body.Add(Create(property));
                    }
                    
                    break;
                }
                case ResourceRef.RefMode.Direct: {
                    var property = serializedObject.GetProperty(nameof(ResourceRef<TData>.DirectReference));
                    body.Add(Create(property));
                    break;
                }
            }
        }

        private void ShowMenu() {
            var property = serializedObject.GetProperty(nameof(ResourceRef.Mode));
            var menu = new ContextMenu();

            void AddOption(string name, string icon, ResourceRef.RefMode mode) {
                var option = menu.AddOption(name, icon, () => {
                    property.SetValue(mode);
                    typeButton.Icon = icon;
                    RebuildControl();
                });
                option.Checkable = true;
                option.Checked = property.GetValue<ResourceRef.RefMode>() == mode;
            }
            
            AddOption("Named", "list_alt", ResourceRef.RefMode.Named);
            AddOption("Direct", "link", ResourceRef.RefMode.Direct);
            
            menu.OpenNextTo(typeButton, WidgetAnchor.BottomEnd with { AdjustSize = true, ConstrainToScreen = true});
        }
    }

    [CustomEditor(typeof(AnimationRef))]
    public class AnimationRefControl : ResourceRefControl<AnimationRef, AnimationClip> {
        public AnimationRefControl(SerializedProperty property) : base(property) { }

        protected override IEnumerable<string> GetAvailableNames() {
            return Graph.Resources?.Animations.Select(x => x.Name) ?? [];
        }
    }
    
    [CustomEditor(typeof(BoneMaskRef))]
    public class BoneMaskRefControl : ResourceRefControl<BoneMaskRef, BoneMask> {
        public BoneMaskRefControl(SerializedProperty property) : base(property) { }

        protected override IEnumerable<string> GetAvailableNames() {
            return Graph.Resources?.BoneMasks.Select(x => x.Name) ?? [];
        }
    }
}
