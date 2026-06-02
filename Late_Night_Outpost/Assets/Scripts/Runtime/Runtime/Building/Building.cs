// ============================================================================
// Building — Identity tag on a placed building
//
// Sits on the root of a spawned building prefab and names what it is. Pure
// data — no behavior, no interaction. Other systems use it to ask "what
// kind of building is this?" — quests, counters, save/load, achievements.
//
// Interaction is NOT this script's job. To make a building upgradeable,
// add a child GameObject with a Focusable + BuildingSite that references
// the next-tier BuildingData. That child becomes the upgrade focus target
// and does the upgrade work when interacted with.
//
// A building with no child BuildingSite is terminal (no upgrade path).
// Multiple child BuildingSites = a fork in the upgrade tree. Designers
// get all of that for free without changing this script.
// ============================================================================

using UnityEngine;

namespace Ludocore
{
    /// <summary>Identity marker on a placed building. Holds the BuildingData
    /// it was spawned from and self-registers into the BuildingRegistry so
    /// workers and HUD can find it. No behavior — interaction lives elsewhere.</summary>
    public class Building : MonoBehaviour
    {
        //==================== CONFIG =====================
        [Header("Identity")]
        [Tooltip("The BuildingData this prefab represents. Wire on the prefab itself.")]
        [SerializeField] private BuildingData data;

        [Header("Registry")]
        [Tooltip("Runtime list of all live buildings. This building registers itself on enable.")]
        [SerializeField] private BuildingRegistry registry;

        public BuildingData Data => data;

        /// <summary>The rally point on this building (a child GameObject with a Rally marker). Null if none.</summary>
        public Rally Rally => GetComponentInChildren<Rally>();

        //==================== LIFECYCLE =====================
        private void OnEnable()  => registry.TryAdd(this);
        private void OnDisable() => registry.Remove(this);
    }
}

// ============================================================================
// Setup on a building prefab
//   1. Add this Building component to the root of the prefab.
//   2. Drag the matching BuildingData asset into the data field.
//   3. Drag the BuildingRegistry asset into the registry field.
//   4. (Optional, for worker rally) Add a child empty GameObject placed where
//      workers should gather — add a Rally component to it.
//   5. (Optional, for the rise-from-ground intro) Add a BuildingEmerge component.
//   6. (Optional, for upgrades) Add a child GameObject with a small Collider,
//      Focusable, and BuildingSite that references the upgrade BuildingData.
//      Wire that site's builtEvent to Destroy this Building's GameObject so
//      the old tier is removed when the new one spawns.
// ============================================================================
