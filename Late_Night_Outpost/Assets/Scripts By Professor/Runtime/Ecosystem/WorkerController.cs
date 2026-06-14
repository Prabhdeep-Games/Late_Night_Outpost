using System;
using UnityEngine;
using UnityEngine.Events;

namespace Ludocore
{
    /// <summary>Worker behavior: walk out to the rally point, forage there (harvest / seek / wander),
    /// and return to the home building when ordered. Day/Night events drive GoOut() / GoHome().
    /// Identity (registry membership) lives on the sibling Worker component.</summary>
    public class WorkerController : MonoBehaviour
    {
        private enum Phase { Deploying, Foraging, Returning }

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

        [Header("Rally")]
        [Tooltip("Tag of the GameObject the worker walks to on spawn and at dawn. RTS-style rally point. Looked up once via FindWithTag in OnEnable.")]
        [SerializeField] private string rallyTag = "Rally";
        [Tooltip("Tag of the GameObject the worker returns to at dusk (the home building).")]
        [SerializeField] private string homeTag = "Home";

        //==================== STATE =====================
        [Header("Debug")]
        [ReadOnly, SerializeField] private Phase phase;
        [ReadOnly, SerializeField] private string currentBehavior;
        [ReadOnly, SerializeField] private Transform rally;
        [ReadOnly, SerializeField] private Transform home;

        //==================== OUTPUTS =====================
        public event Action<IHarvestable> OnHarvested;

        [Header("Events")]
        [Tooltip("Fired when this worker successfully harvests a target.")]
        [SerializeField] private UnityEvent harvestedEvent;

        //==================== LIFECYCLE =====================
        private void OnEnable()
        {
            var rallyGO = GameObject.FindGameObjectWithTag(rallyTag);
            var homeGO  = GameObject.FindGameObjectWithTag(homeTag);
            rally = rallyGO ? rallyGO.transform : null;
            home  = homeGO  ? homeGO.transform  : null;

            GoOut();
        }

        private void Update()
        {
            if (!lifecycle.IsAlive) return;

            switch (phase)
            {
                case Phase.Deploying:
                    currentBehavior = "Deploying";
                    if (motor.HasArrived) EnterForaging();
                    break;

                case Phase.Foraging:
                    if      (TryHarvest()) currentBehavior = "Harvest";
                    else if (TrySeek())    currentBehavior = "Seek";
                    else                   currentBehavior = "Wander";
                    wander.enabled = currentBehavior == "Wander";
                    break;

                case Phase.Returning:
                    currentBehavior = "Returning";
                    // Motor reaches home and stops on its own.
                    break;
            }
        }

        //==================== INPUTS =====================
        /// <summary>Send the worker out to the rally point. Wire this to the onDawn event.</summary>
        [ContextMenu("Go Out")]
        public void GoOut()
        {
            phase = Phase.Deploying;
            wander.enabled = false;
            if (rally) motor.MoveTo(rally.position);
        }

        /// <summary>Send the worker home. Wire this to the onDusk event.</summary>
        [ContextMenu("Go Home")]
        public void GoHome()
        {
            phase = Phase.Returning;
            wander.enabled = false;
            if (home) motor.MoveTo(home.position);
        }

        //==================== PRIVATE =====================
        private void EnterForaging()
        {
            phase = Phase.Foraging;
        }

        private bool TryHarvest()
        {
            if (!targeting.HasTarget) return false;
            if (targeting.Distance > harvestRange) return false;

            motor.Stop();

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
//      into it — single source of truth for "where's a tree?".
//   3. Add a Worker (identity) component. Wire the WorkerRegistry asset.
//   4. Add a Timer child/sibling for the harvest cooldown.
//      Set autoPlay = false, ticks = 1, duration = desired cooldown.
//   5. Add this WorkerController. Wire Lifecycle, Motor, Wander, Targeting,
//      Timer, and the behavior values.
//
//   Rally / Home (new)
//   6. Make sure the project has two tags: "Rally" and "Home" (Tags & Layers).
//   7. Drop an empty GameObject in the scene at the field marker spot — tag it
//      "Rally". The worker walks here on spawn and at dawn.
//   8. Tag the home building GameObject "Home". The worker returns here at dusk.
//      (Single rally / single home for now — FindWithTag returns the first.)
//
//   Day / Night wiring
//   9. On the worker prefab, add two GameEventListener components:
//      - One listens to onDawn  → wire UnityEvent → WorkerController.GoOut()
//      - One listens to onDusk  → wire UnityEvent → WorkerController.GoHome()
//
//  10. (Later) Drop a Spawner on a building and assign the worker prefab —
//      same module fauna use to replicate.
// ============================================================================
