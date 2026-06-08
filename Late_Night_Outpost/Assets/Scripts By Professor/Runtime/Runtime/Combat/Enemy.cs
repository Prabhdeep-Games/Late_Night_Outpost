// ============================================================================
// Enemy — Identity component
//
// Marks this GameObject as an Enemy and self-registers to an EnemyRegistry.
// Other systems (wave UI, win/lose, mini-map pings, AI lookups) can then find
// live enemies via the registry without scene scanning.
//
// Keep this script tiny — it has no brain. Behavior lives on a sibling
// EnemyController (cascade) or StateMachine + states on the same prefab.
//
// (Previously this script was both identity AND brain. The brain has moved to
// EnemyController.cs — same logic, different file. The split lets us show the
// subscription pattern cleanly and lets the FSM prefab swap out the brain
// without touching identity.)
// ============================================================================

using UnityEngine;

namespace Ludocore
{
    /// <summary>Identity component. Marks this GameObject as an enemy and registers it with EnemyRegistry.</summary>
    public class Enemy : MonoBehaviour
    {
        [Tooltip("The registry to register into on enable.")]
        [SerializeField] private EnemyRegistry registry;

        private void OnEnable()
        {
            registry.TryAdd(this);
        }

        private void OnDisable()
        {
            registry.Remove(this);
        }
    }
}
