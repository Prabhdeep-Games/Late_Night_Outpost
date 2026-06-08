// ============================================================================
// EnemyController — Logic layer
//
// Decides what the enemy does each frame: attack if a target is in range, seek
// otherwise, idle if nothing is targetable at all. Target finding is delegated
// to PriorityTargeting (priority-tagged scan with Core fallback) so this
// controller is purely about deciding what to do.
//
// Attack is delegated to a Spawner: when in range, the enemy faces the target
// and calls Spawn() each frame — the Spawner self-gates with its own cooldown
// so attack cadence is a knob on the Spawner, not here. The spawned projectile
// owns the damage application (e.g. a moving prefab with a DamageZone).
//
// No Lifecycle module — enemies don't decay, they die from damage. HealthSystem
// on the same prefab handles HP and IDamageable.
//
// Identity (registry membership) lives on the sibling Enemy component.
// ============================================================================

using UnityEngine;

namespace Ludocore
{
    /// <summary>Marches toward the Core (via PriorityTargeting's fallback) and attacks higher-priority targets it sees on the way. Identity is on the sibling Enemy component.</summary>
    public class EnemyController : MonoBehaviour
    {
        //==================== CONFIG =====================
        [Header("Modules")]
        [Tooltip("Motor used for NavMesh-based movement.")]
        [SerializeField] private NavMeshMotor motor;
        [Tooltip("Finds the highest-priority target in range, with the Core as fallback. Replaces the old Sensor + priority-tag find-loop combo.")]
        [SerializeField] private PriorityTargeting targeting;
        [Tooltip("Spawner that emits a projectile each attack tick. Its own cooldown controls attack rate.")]
        [SerializeField] private Spawner attackSpawner;

        [Header("Behavior")]
        [Tooltip("Distance at which the enemy stops moving and starts attacking.")]
        [Min(0f)]
        [SerializeField] private float attackRange = 4f;

        //==================== STATE =====================
        [Header("Debug")]
        [ReadOnly, SerializeField] private string currentBehavior;

        //==================== LIFECYCLE =====================
        private void Update()
        {
            if (!targeting.HasTarget)
            {
                //motor.Stop();
                currentBehavior = "Idle";
                return;
            }

            if (targeting.Distance > attackRange)
            {
                motor.MoveTo(targeting.Target.position);
                Debug.Log(targeting.Target.name);
                currentBehavior = "Seek";
            }
            else
            {
                motor.Stop();
                FaceFlat(targeting.Target.position);
                attackSpawner.Spawn(); // Spawner self-gates via its own cooldown.
                currentBehavior = "Attack";
            }
        }

        //==================== PRIVATE =====================
        // Flat look — keep the enemy upright. Avoids the projectile firing
        // angled up or down when the target's pivot is at a different height.
        private void FaceFlat(Vector3 worldPos)
        {
            worldPos.y = transform.position.y;
            if (worldPos != transform.position) transform.LookAt(worldPos);
        }
    }
}

// ============================================================================
// Setup in a scene
//   1. Tag the hub building "Core" (single tag — don't also tag it "Building").
//   2. Tag the player "Player", workers "Worker", attackable buildings
//      "Building". Make sure those tags exist in Tags & Layers.
//   3. Create an EnemyRegistry asset:
//      Right-click → Create → Ludocore → Registries → Enemy Registry.
//   4. Build the Enemy prefab from:
//      NavMeshAgent + HealthSystem + NavMeshMotor + a Sensor (filtered to the
//      priority tags) + a Spawner aimed forward with a projectile prefab.
//   5. Add a PriorityTargeting component. Wire the sensor, set priorityTags
//      (e.g. ["Player", "Worker", "Building"]), set coreTag = "Core".
//   6. Add an Enemy (identity) component. Wire the EnemyRegistry asset.
//   7. Add this EnemyController. Wire Motor, Targeting, Spawner, attackRange.
//   8. The attack rate is the Spawner's Cooldown field. The projectile prefab
//      owns its own damage (e.g. moves forward + has a DamageZone with a
//      TriggerSensor filtered to the appropriate tags).
// ============================================================================
