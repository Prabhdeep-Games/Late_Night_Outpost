// ============================================================================
// BuildingRegistry — Runtime registry
//
// Concrete RuntimeSet<Building> asset. Every live Building registers itself
// here on enable and removes itself on disable. Other systems (worker rally
// lookup, HUD counts, win/lose checks) read the list or subscribe to
// OnItemAdded / OnItemRemoved without FindObjectsOfType or static lists.
// ============================================================================

using UnityEngine;

namespace Ludocore
{
    [CreateAssetMenu(menuName = "Ludocore/Registries/Building Registry", fileName = "BuildingRegistry")]
    public class BuildingRegistry : RuntimeSet<Building> { }
}
