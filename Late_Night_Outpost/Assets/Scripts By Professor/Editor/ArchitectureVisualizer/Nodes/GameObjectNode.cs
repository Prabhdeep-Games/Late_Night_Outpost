// ============================================================================
// GameObjectNode — visual node for a GameObject in the scene.
// Title bar is neutral grey to read as a "container" rather than a "script".
// ============================================================================

using UnityEngine;

namespace Ludocore.Editor.ArchitectureVisualizer
{
    public class GameObjectNode : ArchitectureNode
    {
        public GameObjectNode(GraphNode data) : base(data) { }

        protected override Color TitleColor => ArchitectureColors.GameObject;

        protected override string BuildSubtitle(GraphNode data) => "GameObject";
    }
}
