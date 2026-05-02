using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using MANIFOLD.AnimGraph.Jobs;
using MANIFOLD.Inspector;
using Sandbox;

namespace MANIFOLD.AnimGraph.Nodes {
    /// <summary>
    /// Selects an option via a source.
    /// </summary>
    [Category(JobCategories.SEQUENCING)]
    [ExposeToAnimGraph]
    public class Selector : JobNode {
        public enum SelectorMode { Bool, Int, Tag }
        
        [Input, Hide]
        public NodeRef[] Options { get; set; } = new NodeRef[2];

        [UpdatesInputs, ToolChange(nameof(ResizeArray))]
        public SelectorMode Mode { get; set; }
        
        [Title("Source"), ShowIf(nameof(Mode), SelectorMode.Bool), Space]
        public ParameterRef<bool> BoolParameter { get; set; }
        [Title("Source"), ShowIf(nameof(Mode), SelectorMode.Int), Space]
        public ParameterRef<int> IntParameter { get; set; }
        [Title("Source"), ShowIf(nameof(Mode), SelectorMode.Tag), Space]
        public TagRef Tag { get; set; }

        [ShowIf(nameof(Mode), SelectorMode.Int), UpdatesInputs, ToolChange(nameof(ResizeArray))]
        public int IntMinValue { get; set; }

        [ShowIf(nameof(Mode), SelectorMode.Int), UpdatesInputs, ToolChange(nameof(ResizeArray))]
        public int IntMaxValue { get; set; }

        [Space]
        public float TransitionDuration { get; set; } = 0.5f;
        public Curve TransitionCurve { get; set; } = Curve.EaseOut;
        public bool ResetOnChange { get; set; } = true;

        [Hide, JsonIgnore]
        public override string DisplayName => $"{Mode} Selector";
        [Hide, JsonIgnore]
        public override Color AccentColor => JobCategories.SEQUENCING_COLOR;
        
        public override IBaseAnimJob CreateJob(JobCreationContext ctx) {
            switch (Mode) {
                case SelectorMode.Bool: {
                    var job = new BoolSelectorJob(ID);
                    if (BoolParameter.IsValid()) {
                        job.SourceParameter = ctx.parameters.Get<bool>(BoolParameter.ID.Value);
                    }
                    job.TransitionDuration = TransitionDuration;
                    job.TransitionCurve = TransitionCurve;
                    job.ResetOnChange = ResetOnChange;
                    return job;
                }
                case SelectorMode.Int: {
                    var count = Math.Max(0, IntMaxValue - IntMinValue + 1);
                    var job = new IntSelectorJob(ID, count);
                    if (IntParameter.IsValid()) {
                        job.Source = ctx.parameters.Get<int>(IntParameter.ID.Value);
                    }
                    if (count != 0) {
                        for (int i = 0; i < count; i++) {
                            job.Values[i] = IntMinValue + i;
                        }
                    }
                    job.TransitionDuration = TransitionDuration;
                    job.TransitionCurve = TransitionCurve;
                    job.ResetOnChange = ResetOnChange;
                    return job;
                }
                case SelectorMode.Tag: {
                    var job = new BoolSelectorJob(ID);
                    if (Tag.IsValid()) {
                        job.SourceTag = ctx.tags.Get(Tag.ID.Value);
                    }
                    job.TransitionDuration = TransitionDuration;
                    job.TransitionCurve = TransitionCurve;
                    job.ResetOnChange = ResetOnChange;
                    return job;
                }
                default: {
                    throw new NotImplementedException();
                }
            }
        }

        public override IEnumerable<NodeRef> GetInputs() {
            return Options;
        }

        private void ResizeArray() {
            var temp = Options;
            switch (Mode) {
                case SelectorMode.Bool: {
                    Array.Resize(ref temp, 2);
                    break;
                }
                case SelectorMode.Int: {
                    int length = Math.Max(0, IntMaxValue - IntMinValue + 1);
                    Array.Resize(ref temp, length);
                    break;
                }
                case SelectorMode.Tag: {
                    Array.Resize(ref temp, 2);
                    break;
                }
            }
            Options = temp;
        }
    }
}
