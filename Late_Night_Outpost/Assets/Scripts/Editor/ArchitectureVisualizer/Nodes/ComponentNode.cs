// ============================================================================
// ComponentNode — visual node for a user-defined MonoBehaviour.
// Title bar is blue. Title shows the script type; subtitle shows the GameObject
// the component lives on so students can locate it in the Hierarchy.
// ============================================================================

using UnityEngine;

namespace Ludocore.Editor.ArchitectureVisualizer
{
    public class ComponentNode : ArchitectureNode
    {
        public ComponentNode(GraphNode data) : base(data) { }

        protected override Color TitleColor => ArchitectureColors.Component;

        protected override string BuildTitle(GraphNode data) => data.DisplayName; // type name

        protected override string BuildSubtitle(GraphNode data)
        {
            var component = data.UnityObject as UnityEngine.Component;
            return component != null ? $"on {component.gameObject.name}" : "Component";
        }
    }
}
