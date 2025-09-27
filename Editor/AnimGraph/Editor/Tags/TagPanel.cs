using Editor;
using Sandbox;

namespace MANIFOLD.AnimGraph.Editor {
    public class TagPanel : Widget {
        public const string EVENT_PREFIX = $"{AnimGraphEditor.EVENT_PREFIX}.tags";
        public const string EVENT_REFRESH = $"{EVENT_PREFIX}.refresh";
        
        private readonly AnimGraphEditor editor;
        private readonly ScrollArea scroll;
        private readonly Widget scrollCanvas;
        
        public TagPanel(AnimGraphEditor editor) : base(editor) {
            this.editor = editor;
            
            Name = "TagList";
            WindowTitle = "Tags";

            Layout = Layout.Column();
            Layout.Margin = 2;
            Layout.Spacing = 4;

            var row = Layout.AddRow();
            row.Add(new Button("Add", "add") { Clicked = ShowAddMenu });
            row.AddStretchCell();

            scroll = new ScrollArea(this);
            
            scrollCanvas = new Widget(scroll);
            scrollCanvas.Layout = Layout.Column();
            scrollCanvas.Layout.AddStretchCell();
            
            scroll.Canvas = scrollCanvas;
            Layout.Add(scroll);
        }

        [Event(AnimGraphEditor.EVENT_GRAPH_LOAD)]
        [Event(AnimGraphEditor.EVENT_PREVIEW)]
        [Event(EVENT_REFRESH)]
        public void Rebuild() {
            scrollCanvas.Layout.Clear(true);
            
            foreach (var tag in editor.GraphResource.Tags.Values) {
                var tagWidget = new TagWidget(editor);
                tagWidget.Tag = tag;
                if (editor.InPreview) {
                    tagWidget.PreviewTag = editor.PreviewAnimator.TagList.Get(tag.ID);
                }
                scrollCanvas.Layout.Add(tagWidget);
            }
            scrollCanvas.Layout.AddStretchCell();
        }

        private void ShowAddMenu() {
            ContextMenu menu = new ContextMenu();

            menu.AddOption("Event Tag", action: () => AddTag(Tag.TagType.Event));
            menu.AddOption("Internal Tag", action: () => AddTag(Tag.TagType.Internal));
            
            menu.OpenAtCursor();
        }

        private void AddTag(Tag.TagType type) {
            var tag = new Tag();
            tag.Type = type;
            editor.GraphResource.Tags.Add(tag.ID, tag);
            editor.GraphResource.StateHasChanged();
            Rebuild();
        }
    }
}
