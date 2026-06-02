// ============================================================================
// ResourceData — Data layer
//
// A ScriptableObject that defines a resource type: Wood, Stone, Food…
// One asset per type, referenced everywhere — HUD rows, harvestables, building
// costs (later). Change the asset once and every reference updates.
//
// Teaches: the Flyweight pattern — one piece of shared, immutable data,
// many users. Also reinforces "data lives in ScriptableObjects, not code."
// ============================================================================

using UnityEngine;

namespace Ludocore
{
    /// <summary>A resource type (Wood, Stone, Food). One asset, many references.</summary>
    [CreateAssetMenu(fileName = "NewResource", menuName = "Ludocore/Resource Data")]
    public class ResourceData : ScriptableObject
    {
        //==================== CONFIG =====================
        [Header("Config")]
        [Tooltip("Display name shown in HUD rows and harvest prompts.")]
        [SerializeField] private string displayName;

        [Tooltip("Icon shown in the HUD (optional).")]
        [SerializeField] private Sprite icon;

        [Tooltip("Tint color for HUD text and UI accents.")]
        [SerializeField] private Color color = Color.white;

        public string DisplayName => displayName;
        public Sprite Icon => icon;
        public Color Color => color;
    }
}

// ============================================================================
// Creating one
//   1. Right-click in the Project window → Create → Ludocore → Resource Data.
//   2. Name it after the resource (e.g. Wood.asset, Stone.asset).
//   3. Fill in the display name, optional icon, and a color.
//   4. Reference this asset from any Harvestable and from the ResourceUI list.
// ============================================================================
