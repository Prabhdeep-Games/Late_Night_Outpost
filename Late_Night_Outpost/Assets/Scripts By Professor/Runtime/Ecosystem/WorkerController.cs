using System;
using UnityEngine;
using UnityEngine.Events;

namespace Ludocore
{
    /// <summary>Worker behavior: seek a matching harvestable, harvest it on cooldown, gain energy.
    /// Same shape as before — but target finding is now delegated to HarvestableTargeting so
    /// this controller is purely about deciding what to do, not how to find a tree.
    /// Identity (registry membership) lives on the sibling Worker component.</summary>
    public class WorkerController : MonoBehaviour
    {
        //==================== CONFIG =====================
        [Header("Modules")]
        [Tooltip("Lifecycle component managing this worker's energy.")]
        [SerializeField] private Lifecycle lifecycle;
        [Tooltip("Motor used for NavMesh-based movement.")]
        [SerializeField] private NavMeshMotor motor;
        [Tooltip("Wander behavior used when no target is found.")]
        [SerializeField] private NavMeshWander wander;
        [Tooltip("Finds the nearest harvestable yielding the configured resource. Replaces the old Sensor + ResourceData + find-loop combo.")]
        [SerializeField] private HarvestableTargeting targeting;
        [Tooltip("Cooldown timer between harvests. Set autoPlay = false, ticks = 1.")]
        [SerializeField] private Timer harvestCooldown;

        [Header("Behavior")]
        [Tooltip("Distance at which the worker can harvest its target.")]
        [Min(0f)]
        [SerializeField] private float harvestRange = 1.5f;
        [Tooltip("Energy gained per successful harvest.")]
        [Min(0f)]
        [SerializeField] private float energyPerHarvest = 20f;

        //==================== STATE =====================
        [Header("Debug")]
        [ReadOnly, SerializeField] private string currentBehavior;

        //==================== OUTPUTS =====================
        public event Action<IHarvestable> OnHarvested;

        [Header("Events")]
        [Tooltip("Fired when this worker successfully harvests a target.")]
        [SerializeField] private UnityEvent harvestedEvent;

        //==================== LIFECYCLE =====================
        private void Update()
        {
            if (!lifecycle.IsAlive) return;

            if      (TryHarvest()) currentBehavior = "Harvest";
            else if (TrySeek())    currentBehavior = "Seek";
            else                   currentBehavior = "Wander";

            wander.enabled = currentBehavior == "Wander";
        }

        //==================== PRIVATE =====================
        private bool TryHarvest()
        {
            if (!targeting.HasTarget) return false;
            if (targeting.Distance > harvestRange) return false;

            // In range but still on cooldown — stay in Harvest mode, don't fire yet.
            if (harvestCooldown.IsRunning) return true;

            targeting.Harvestable.Harvest();
            lifecycle.AddEnergy(energyPerHarvest);
            harvestCooldown.Restart();

            OnHarvested?.Invoke(targeting.Harvestable);
            harvestedEvent?.Invoke();
            return true;
        }

        private bool TrySeek()
        {
            if (!targeting.HasTarget) return false;
            motor.MoveTo(targeting.Target.position);
            return true;
        }
    }
}

// ============================================================================
// Setup in a scene
//   1. Build the worker prefab from the same modules as a fauna entity:
//      NavMeshAgent + Lifecycle + NavMeshMotor + NavMeshWander + a Sensor
//      (e.g. ProximitySensor).
//   2. Add a HarvestableTargeting component. Wire the sensor + ResourceData
//      into it — this becomes the single source of truth for "where's a tree?".
//   3. Add a Worker (identity) component. Wire the WorkerRegistry asset.
//   4. Add a Timer child/sibling for the harvest cooldown.
//      Set autoPlay = false, ticks = 1, duration = desired cooldown.
//   5. Add this WorkerController. Wire Lifecycle, Motor, Wander, Targeting,
//      Timer, and the behavior values.
//   6. (Later) Drop a Spawner on a building and assign the worker prefab —
//      same module fauna use to replicate.
// ============================================================================
