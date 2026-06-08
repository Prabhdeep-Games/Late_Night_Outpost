// ============================================================================
// ScriptableObjectNode — visual node for any SO referenced from the scene.
// Title bar is green. Title shows the asset name; subtitle shows the SO type.
// ============================================================================

using UnityEngine;

namespace Ludocore.Editor.ArchitectureVisualizer
{
    public class ScriptableObjectNode : ArchitectureNode
    {
        public ScriptableObjectNode(GraphNode data) : base(data) { }

        protected override Color TitleColor => ArchitectureColors.ScriptableObject;

        protected override string BuildSubtitle(GraphNode data) => data.TypeName;
    }
}
