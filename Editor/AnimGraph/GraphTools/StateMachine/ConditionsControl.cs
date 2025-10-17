using System;
using System.Collections.Generic;
using System.Linq;
using Editor;
using MANIFOLD.AnimGraph.Editor;
using MANIFOLD.AnimGraph.Nodes;
using MANIFOLD.Utility;
using Sandbox;
using Sandbox.UI;

namespace MANIFOLD.AnimGraph.GraphTools {
    [CustomEditor(typeof(List<StateMachine.Condition>))]
    public class ConditionsControl : ControlWidget {
        public class ConditionWidget : Widget {
            private readonly ConditionsControl control;
            public readonly StateMachine.Condition condition;
            public readonly SerializedObject serializedObject;
            private ComboBox modeSelection;
            private ComboBox comparisonSelection;
            private ControlWidget controlWidget;
            
            public ConditionWidget(ConditionsControl control, StateMachine.Condition condition) {
                this.control = control;
                this.condition = condition;
                serializedObject = condition.GetSerialized();

                FocusMode = FocusMode.Click;

                Layout = Layout.Row();
                Layout.Margin = new Margin(32, 2, 2, 2);
                Layout.Spacing = 1;

                modeSelection = Layout.Add(new ComboBox());
                comparisonSelection = Layout.Add(new ComboBox());
                comparisonSelection.FixedWidth = 60;
                
                Rebuild();
            }

            protected override void OnFocus(FocusChangeReason reason) {
                control.active = this;
            }

            protected override void OnPaint() {
                Paint.Antialiasing = true;
                
                if (IsFocused) {
                    Paint.SetBrushAndPen(Theme.Blue);
                    Paint.DrawRect(LocalRect);
                }

                Vector2 centerPoint = new Vector2(15, Height * 0.5f);
                Vector2 size = Height * 0.6f;
                
                Paint.SetPen(Color.White, size: size.x * 0.14f);
                Paint.ClearBrush();
                Paint.DrawCircle(centerPoint, size); 
                
                Paint.ClearPen();
                Paint.SetBrush(condition.Color);
                Paint.DrawCircle(centerPoint, size * 0.6f);
            }
            
            [Event(InspectorPanel.EVENT_REBUILD)]
            public void Rebuild() {
                if (condition is StateMachine.ParameterCondition paramCond) {
                    RebuildParameter(paramCond);
                } else if (condition is StateMachine.TagCondition tagCond) {
                    RebuildTag(tagCond);
                } else {
                    RebuildNormal();
                }
            }

            private void RebuildNormal() {
                modeSelection.Clear();
                comparisonSelection.Clear();
                controlWidget?.Destroy();
                controlWidget = null;
                
                foreach (var modes in condition.AvailableModes) {
                    modeSelection.AddItem(modes.Key, selected: modes.Value.Equals(condition.CurrentMode), onSelected: () => condition.CurrentMode = modes.Value);
                }
                
                var displays = DisplayInfo.ForEnumValues<StateMachine.Comparison>();
                foreach (StateMachine.Comparison comparison in Enum.GetValues(typeof(StateMachine.Comparison))) {
                    if (comparison == StateMachine.Comparison.None || comparison == StateMachine.Comparison.All) continue;

                    if (condition.SupportedComparisons.HasFlag(comparison)) {
                        comparisonSelection.AddItem(displays.First(x => x.value == comparison).info.Name, onSelected: () => condition.CurrentComparison = comparison);
                    }
                }
                
                var property = serializedObject.GetProperty(condition.TestValueName);
                controlWidget = Create(property);
                Layout.Add(controlWidget);
                StyleControl();
            }

            private void RebuildParameter(StateMachine.ParameterCondition cond) {
                modeSelection.Clear();
                
                var graph = GetContext<AnimGraph>(AnimGraphEditor.CONTEXT_GRAPH);
                if (graph == null) {
                    modeSelection.AddItem("Unavailable");
                    return;
                }
                
                modeSelection.AddItem("<None>", onSelected: () => cond.Parameter = null);
                foreach (var param in graph.Parameters.Values) {
                    modeSelection.AddItem(param.Name, selected: cond.Parameter == param.ID, onSelected: () => {
                        cond.Parameter = param.ID;
                        RebuildParameterControls(cond, true);
                    });
                }

                RebuildParameterControls(cond, false);
            }

            private void RebuildParameterControls(StateMachine.ParameterCondition cond, bool newParam) {
                comparisonSelection.Clear();
                controlWidget?.Destroy();
                controlWidget = null;
                
                var graph = GetContext<AnimGraph>(AnimGraphEditor.CONTEXT_GRAPH);
                if (graph == null) return;
                
                Parameter selected = null;
                var availableComparisons = StateMachine.Comparison.None;
                
                if (cond.Parameter.HasValue) {
                    selected = graph.Parameters[cond.Parameter.Value];

                    if (selected != null) {
                        var equatableType = typeof(IEquatable<>);
                        equatableType = equatableType.MakeGenericType(selected.DataType);
                        if (selected.DataType.IsAssignableTo(equatableType)) {
                            availableComparisons |= StateMachine.Comparison.Equal | StateMachine.Comparison.NotEqual;
                        }
                        
                        var comparableType = typeof(IComparable<>);
                        comparableType = comparableType.MakeGenericType(selected.DataType);
                        if (selected.DataType != typeof(bool) && selected.DataType.IsAssignableTo(comparableType)) {
                            availableComparisons |= StateMachine.Comparison.GreaterThan |
                                                    StateMachine.Comparison.GreaterThanOrEqual |
                                                    StateMachine.Comparison.LessThan |
                                                    StateMachine.Comparison.LessThanOrEqual;
                        }

                        if (newParam) {
                            cond.TestValue = Activator.CreateInstance(selected.DataType);
                        }
                    }
                }
                
                var displays = DisplayInfo.ForEnumValues<StateMachine.Comparison>();
                foreach (StateMachine.Comparison comparison in Enum.GetValues(typeof(StateMachine.Comparison))) {
                    if (comparison == StateMachine.Comparison.None || comparison == StateMachine.Comparison.All) continue;

                    if (availableComparisons.HasFlag(comparison)) {
                        comparisonSelection.AddItem(displays.First(x => x.value == comparison).info.Name, onSelected: () => condition.CurrentComparison = comparison);
                    }
                }

                if (selected != null) {
                    var realProperty = serializedObject.GetProperty(condition.TestValueName);
                    var wrappedProperty = realProperty.ChangeType(selected.DataType);
                    
                    controlWidget = Layout.Add(Create(wrappedProperty));
                    
                    StyleControl();
                }
            }

            private void RebuildTag(StateMachine.TagCondition cond) {
                modeSelection.Clear();
                comparisonSelection.Clear();
                controlWidget?.Destroy();
                controlWidget = null;
                
                var graph = GetContext<AnimGraph>(AnimGraphEditor.CONTEXT_GRAPH);
                if (graph == null) {
                    modeSelection.AddItem("Unavailable");
                    return;
                }
                
                modeSelection.AddItem("<None>", onSelected: () => cond.Tag = null);
                foreach (var param in graph.Tags.Values) {
                    modeSelection.AddItem(param.Name, selected: cond.Tag == param.ID, onSelected: () => {
                        cond.Tag = param.ID;
                    });
                }
                
                var displays = DisplayInfo.ForEnumValues<StateMachine.Comparison>();
                foreach (StateMachine.Comparison comparison in Enum.GetValues(typeof(StateMachine.Comparison))) {
                    if (comparison == StateMachine.Comparison.None || comparison == StateMachine.Comparison.All) continue;

                    if (condition.SupportedComparisons.HasFlag(comparison)) {
                        comparisonSelection.AddItem(displays.First(x => x.value == comparison).info.Name, onSelected: () => condition.CurrentComparison = comparison);
                    }
                }
                
                var property = serializedObject.GetProperty(condition.TestValueName);
                controlWidget = Create(property);
                Layout.Add(controlWidget);
                StyleControl();
            }
            
            private void StyleControl() {
                if (controlWidget != null) {
                    controlWidget.FixedWidth = 200f;
                }
            }
        }

        private SerializedCollection serialized;
        private readonly ScrollArea scroll;
        private readonly Widget canvas;
        private List<ConditionWidget> widgets;
        private ConditionWidget active;
        
        public ConditionsControl(SerializedProperty property) : base(property) {
            MinimumHeight = 100;
            
            Layout = Layout.Column();
            Layout.Spacing = 6;

            if (!property.TryGetAsObject(out var obj)) {
                Log.Error("failed to convert conditions");
                return;
            }
            serialized = (SerializedCollection)obj;
            
            var row = Layout.AddRow();
            row.Spacing = 4;
            row.AddStretchCell();
            row.Add(new IconButton("add", OpenAddMenu));
            row.Add(new IconButton("delete", DeleteActive));

            scroll = Layout.Add(new ScrollArea(this));
            scroll.SetSizeMode(SizeMode.Default, SizeMode.Flexible);
            canvas = new Widget();
            canvas.Layout = Layout.Column();
            widgets = new List<ConditionWidget>();
            
            scroll.Canvas = canvas;

            Rebuild();
        }

        protected override void PaintUnder() {
            
        }

        private void Rebuild() {
            canvas.Layout.Clear(true);
            widgets.Clear();

            foreach (var obj in serialized) {
                var cond = obj.GetValue<StateMachine.Condition>();
                var widget = canvas.Layout.Add(new ConditionWidget(this, cond));
                widgets.Add(widget);
            }
            canvas.Layout.AddStretchCell();
        }
        
        private void OpenAddMenu() {
            var menu = new ContextMenu();

            foreach (var type in EditorTypeLibrary.GetTypes<StateMachine.Condition>()) {
                if (type.TargetType == typeof(StateMachine.Condition)) continue;
                menu.AddOption(type.Title, action: () => AddCondition(type));
            }
            
            menu.OpenAtCursor(true);
        }

        private void DeleteActive() {
            if (active == null) return;
            
            int index = widgets.IndexOf(active);
            serialized.RemoveAt(index);
            
            Rebuild();
        }

        private void AddCondition(TypeDescription type) {
            var cond = type.Create<StateMachine.Condition>();
            serialized.Add(cond);
            Rebuild();
        }
    }
}
