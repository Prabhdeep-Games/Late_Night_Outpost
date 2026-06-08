// ============================================================================
// WorkerRegistry — Runtime registry
//
// Concrete RuntimeSet<Worker> asset. Every live Worker registers itself here
// on enable and removes itself on disable. Other systems (HUD counts, mini-map
// pings, AI lookups, spawn caps) read the list or subscribe to OnItemAdded /
// OnItemRemoved without FindObjectsOfType or static lists.
// ============================================================================

using UnityEngine;

namespace Ludocore
{
    [CreateAssetMenu(menuName = "Ludocore/Registries/Worker Registry", fileName = "WorkerRegistry")]
    public class WorkerRegistry : RuntimeSet<Worker> { }
}
