// ============================================================================
// Worker — Identity component
//
// Marks this GameObject as a Worker and self-registers to a WorkerRegistry.
// Other systems (HUD counts, spawners, AI) can then find live workers via the
// registry without scene scanning.
//
// Keep this script tiny — it has no brain. Behavior lives on a sibling
// WorkerController (cascade) or StateMachine + states on the same prefab.
// ============================================================================

using UnityEngine;

namespace Ludocore
{
    /// <summary>Identity component. Marks this GameObject as a worker and registers it with WorkerRegistry.</summary>
    public class Worker : MonoBehaviour
    {
        [Tooltip("The registry to register into on enable.")]
        [SerializeField] private WorkerRegistry registry;

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
