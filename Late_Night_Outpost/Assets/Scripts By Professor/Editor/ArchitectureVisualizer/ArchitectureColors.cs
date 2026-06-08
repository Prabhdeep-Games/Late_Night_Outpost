// ============================================================================
// ArchitectureColors — single source of truth for the visualizer's palette.
//
// Node colors (used by the title bars of the three Node subclasses) and edge
// colors (used by the GraphView when drawing connections) live here so the
// legend overlay can reference the same values — what students see in the
// graph and what the legend explains are guaranteed to match.
// ============================================================================

using UnityEngine;

namespace Ludocore.Editor.ArchitectureVisualizer
{
    internal static class ArchitectureColors
    {
        // Node title bars
        public static readonly Color GameObject       = new(0.30f, 0.30f, 0.30f);
        public static readonly Color Component        = new(0.16f, 0.36f, 0.56f);
        public static readonly Color ScriptableObject = new(0.16f, 0.56f, 0.30f);

        // Edge lines
        public static readonly Color Containment      = new(0.45f, 0.45f, 0.45f, 0.40f);
        public static readonly Color ObjectReference  = new(0.75f, 0.85f, 1.00f, 1.00f);
        public static readonly Color DataReference    = new(0.30f, 0.85f, 0.45f, 1.00f);
    }
}
