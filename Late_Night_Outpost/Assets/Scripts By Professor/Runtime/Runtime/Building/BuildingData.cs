// ============================================================================
// BuildingData — Data layer
//
// A ScriptableObject that defines a building type: display name, the prefab
// to spawn, optional cost, and (optionally) the next level in an upgrade chain.
//
// One asset per building. Used by both placement verbs:
//   BuildingSite (in-world) — instantiates Prefab at the site's transform on interact
//   Builder      (held prop) — instantiates Prefab at a raycast target
//
// The spawned Prefab should carry a Building component for identity, plus
// its own visuals / animation / colliders. Upgrade paths can be expressed
// two ways (use either or both):
//   • NextLevel field — a data-driven chain (Hut → House → Manor)
//   • Child BuildingSite on the prefab — places an upgrade-trigger focusable
//     directly on the building, referencing the next-tier BuildingData
// ============================================================================

using System.Collections.Generic;
using UnityEngine;

namespace Ludocore
{
    /// <summary>Building definition — name, prefab, optional cost, optional next level in an upgrade chain.</summary>
    [CreateAssetMenu(fileName = "NewBuildingData", menuName = "Ludocore/Building Data")]
    public class BuildingData : ScriptableObject
    {
        //==================== CONFIG =====================
        [Header("Identity")]
        [Tooltip("Display name shown in the building site prompt.")]
        [SerializeField] private string buildingName;

        [Header("Spawn")]
        [Tooltip("Prefab instantiated when this building is placed. Should carry a Building component plus its visuals / colliders.")]
        [SerializeField] private GameObject prefab;

        [Header("Cost (optional)")]
        [Tooltip("Resources required to build. Leave empty for a free building.")]
        [SerializeField] private List<ResourceCost> costs = new();

        [Header("Upgrade (optional)")]
        [Tooltip("If set, building this level lets the site upgrade to this next data. " +
                 "Leave empty for a terminal building (no further upgrade).")]
        [SerializeField] private BuildingData nextLevel;

        public string BuildingName => buildingName;
        public GameObject Prefab => prefab;
        public IReadOnlyList<ResourceCost> Costs => costs;
        public bool IsFree => costs == null || costs.Count == 0;
        public BuildingData NextLevel => nextLevel;
    }
}
