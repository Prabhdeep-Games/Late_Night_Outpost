// ============================================================================
// ArchitectureNode — shared base for the three visual node types.
//
// Every node:
//   - holds a back-reference to its GraphNode (and its UnityObject)
//   - has one input + one output port (left/right) so edges can connect
//   - pings its UnityObject in the Editor when selected
//   - is not movable through the keyboard / not deletable / not collapsible
//     (read-only feel, but still draggable for layout)
// ============================================================================

using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Ludocore.Editor.ArchitectureVisualizer
{
    public abstract class ArchitectureNode : Node
    {
        public GraphNode Data { get; }
        public Port InputPort { get; private set; }
        public Port OutputPort { get; private set; }

        protected ArchitectureNode(GraphNode data)
        {
            Data = data;
            title = BuildTitle(data);
            tooltip = "Click to ping in Hierarchy / Project";

            ApplyTitleColor(TitleColor);

            string subtitle = BuildSubtitle(data);
            if (!string.IsNullOrEmpty(subtitle))
                AddSubtitle(subtitle);

            InputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input,
                Port.Capacity.Multi, typeof(object));
            InputPort.portName = string.Empty;
            inputContainer.Add(InputPort);

            OutputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output,
                Port.Capacity.Multi, typeof(object));
            OutputPort.portName = string.Empty;
            outputContainer.Add(OutputPort);

            // Read-only feel: no collapse button, no Delete handler will remove it.
            capabilities &= ~Capabilities.Deletable;
            capabilities &= ~Capabilities.Collapsible;
            capabilities &= ~Capabilities.Renamable;

            RefreshExpandedState();
            RefreshPorts();
        }

        protected abstract Color TitleColor { get; }

        protected virtual string BuildTitle(GraphNode data) => data.DisplayName;
        protected virtual string BuildSubtitle(GraphNode data) => null;

        public override void OnSelected()
        {
            base.OnSelected();
            Object obj = Data.UnityObject;
            if (obj == null) return;
            EditorGUIUtility.PingObject(obj);
            Selection.activeObject = obj;
        }

        private void ApplyTitleColor(Color c)
        {
            titleContainer.style.backgroundColor = new StyleColor(c);
            // Tint the title text to high-contrast white.
            var titleLabel = this.Q<Label>("title-label");
            if (titleLabel != null)
                titleLabel.style.color = new StyleColor(Color.white);
        }

        private void AddSubtitle(string text)
        {
            var label = new Label(text);
            label.style.color = new StyleColor(new Color(0.85f, 0.85f, 0.85f));
            label.style.unityFontStyleAndWeight = FontStyle.Italic;
            label.style.paddingLeft = 8;
            label.style.paddingRight = 8;
            label.style.paddingTop = 2;
            label.style.paddingBottom = 4;
            label.style.fontSize = 10;
            extensionContainer.Add(label);
        }
    }
}
