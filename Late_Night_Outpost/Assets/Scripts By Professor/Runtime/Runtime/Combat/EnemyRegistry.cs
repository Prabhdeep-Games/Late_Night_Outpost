// ============================================================================
// EnemyRegistry — Runtime registry
//
// Concrete RuntimeSet<Enemy> asset. Every live Enemy registers itself here on
// enable and removes itself on disable. Other systems (wave UI, win/lose,
// minimap pings) read the list or subscribe to OnItemAdded / OnItemRemoved
// without FindObjectsOfType or static lists.
// ============================================================================

using UnityEngine;

namespace Ludocore
{
    [CreateAssetMenu(menuName = "Ludocore/Registries/Enemy Registry", fileName = "EnemyRegistry")]
    public class EnemyRegistry : RuntimeSet<Enemy> { }
}
