// ============================================================================
// StatueEnemyController — Logic layer for the "living statue" enemy
//
// Same priority-list idea as EnemyController, written as one switch so each
// phase is a single, readable case. Awareness (a Meter) drives the phase; the
// player-detected bit only splits the low band (Triggered vs LookAround) and
// range only gates the Attack. Within a phase the statue goes after the player
// via the composed movement modules.
//
//   awareness == 0            -> Idle (at home) / BackToBase (returning)
//   0 < awareness < threshold -> Triggered (seen) / LookAround (lost)
//   awareness >= threshold    -> Attacking (in range) / Chasing
//
// A SightSensor feeds the Meter (fills while the player is seen, drains
// otherwise). Movement is delegated to NavMeshChase + NavMeshWander; the bare
// NavMeshMotor walks back to the spawn point. Attack is a Spawner: the Attack()
// callback emits one projectile, self-gated by the Spawner's own cooldown.
//
// Every phase exposes a UnityEvent (fired on entry) so animations and other
// scene elements can be wired in the Inspector. The current phase shows in the
// readonly debug field.
// ============================================================================

using System;
using UnityEngine;
using UnityEngine.Events;

namespace Ludocore
{
    public enum StatuePhase { Idle, Triggered, Chasing, Attacking, LookAround, BackToBase, Death }

    /// <summary>Living-statue enemy. Awareness drives the phase; composes a SightSensor + Meter + chase/wander modules + an attack Spawner.</summary>
    public class StatueEnemyController : MonoBehaviour
    {
        //==================== CONFIG =====================
        [Header("Modules")]
        [Tooltip("Sight-based detection. Fills the awareness meter while the player is seen.")]
        [SerializeField] private SightSensor sensor;
        [Tooltip("Awareness meter (0..1). Fills while the player is detected, drains otherwise.")]
        [SerializeField] private Meter meter;
        [Tooltip("Motor used to walk back to the spawn point.")]
        [SerializeField] private NavMeshMotor motor;
        [Tooltip("Chase module — drives movement during the Chasing phase.")]
        [SerializeField] private NavMeshChase chase;
        [Tooltip("Wander module — drives movement during the LookAround phase. Set its radius to the search area.")]
        [SerializeField] private NavMeshWander wander;
        [Tooltip("Spawner fired during the Attacking phase. Its cooldown is the attack rate.")]
        [SerializeField] private Spawner attackSpawner;

        [Header("Behavior")]
        [Tooltip("Awareness at/above which the statue commits to chasing (and attacking). Below it the statue is only triggered/searching.")]
        [Range(0f, 1f)]
        [SerializeField] private float chaseThreshold = 0.5f;

        [Tooltip("Distance at which the statue stops and attacks instead of chasing.")]
        [Min(0f)]
        [SerializeField] private float attackRange = 2f;

        [Tooltip("How close to the spawn point counts as 'home' (ends BackToBase, returns to Idle).")]
        [Min(0f)]
        [SerializeField] private float baseArrivalDistance = 0.5f;

        //==================== STATE =====================
        [Header("Debug")]
        [ReadOnly, SerializeField] private StatuePhase phase = StatuePhase.Idle;

        private Vector3 _homePosition;
        private Transform _target;
        private float _distance = Mathf.Infinity;

        public StatuePhase Phase => phase;

        //==================== OUTPUTS =====================
        public event Action<StatuePhase> OnPhaseChanged;

        [Header("Events")]
        [Tooltip("Entered Idle — immobile at the spawn point.")]
        [SerializeField] private UnityEvent idleEvent;
        [Tooltip("Entered Triggered — comes alive, looking around in place (wire the look-around animation here).")]
        [SerializeField] private UnityEvent triggeredEvent;
        [Tooltip("Entered Chasing — pursuing the player.")]
        [SerializeField] private UnityEvent chasingEvent;
        [Tooltip("Entered Attacking — in range, firing the spawner.")]
        [SerializeField] private UnityEvent attackingEvent;
        [Tooltip("Entered LookAround — lost the player, wandering the search area.")]
        [SerializeField] private UnityEvent lookAroundEvent;
        [Tooltip("Entered BackToBase — awareness spent, walking home.")]
        [SerializeField] private UnityEvent backToBaseEvent;
        [Tooltip("Entered Death — play death animation, disable the enemy.")]
        [SerializeField] private UnityEvent DeathEvent;
        //==================== LIFECYCLE =====================
        private void Awake()
        {
            _homePosition = transform.position;
        }

        private void Start()
        {
            // Neutralize any autoPlay on the movement modules so the switch owns movement.
            StopMovement();
        }

        private void Update()
        {
            DriveAwareness();

            StatuePhase next = DecidePhase();
            bool entered = next != phase;
            phase = next;

            switch (phase)
            {
                case StatuePhase.Idle:
                    if (entered) Enter(idleEvent);
                    break;

                case StatuePhase.Triggered:
                    if (entered) Enter(triggeredEvent); // immobile; look-around via the wired animation
                    break;

                case StatuePhase.Chasing:
                    if (entered) { Enter(chasingEvent); chase.StartChase(_target); }
                    break;

                case StatuePhase.Attacking:
                    if (entered) Enter(attackingEvent);
                    Attack(); // self-gated by the spawner cooldown
                    break;

                case StatuePhase.LookAround:
                    if (entered) { Enter(lookAroundEvent); wander.StartWander(); }
                    break;

                case StatuePhase.BackToBase:
    if (entered)
    {
        Enter(backToBaseEvent);
        motor.MoveTo(_homePosition);
    }

    // Face the current movement direction if there is one.
    var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
    if (agent != null && agent.hasPath && agent.path.corners.Length > 1)
    {
        // Next corner in the path.
        Vector3 nextCorner = agent.path.corners[1];
        FaceFlat(nextCorner);
    }
    else
    {
        // Fallback: face home position.
        FaceFlat(_homePosition);
    }

    break;

                case StatuePhase.Death:
                    if (entered) { Enter(DeathEvent); StopMovement(); }
                    break;
            }
        }

        //==================== INPUTS =====================
        /// <summary>Attack callback — face the target and emit one attack via the spawner (self-gated by its cooldown). Also safe to wire to an attack animation event.</summary>
        public void Attack()
        {
            if (_target) FaceFlat(_target.position);
            attackSpawner.Spawn();
        }

        //==================== PRIVATE =====================
        // Sensor drives the meter: fill while the player is seen, drain otherwise.
        // Remembers the last seen target so chasing survives the meter's drain delay.
        private void DriveAwareness()
        {
            if (sensor.HasDetections && sensor.TryGetNearest(out var nearest))
            {
                _target = nearest.Object.transform;
                _distance = nearest.Distance;
                meter.StartFilling();
            }
            else
            {
                meter.StopFilling();
            }
        }

        // Priority list: awareness first, detection splits the low band, range gates the attack.
        private StatuePhase DecidePhase()
        {
            float awareness = meter.Value;
            bool detected = sensor.HasDetections;

            if (awareness <= 0f)
                return IsHome() ? StatuePhase.Idle : StatuePhase.BackToBase;

            if (awareness < chaseThreshold)
                return detected ? StatuePhase.Triggered : StatuePhase.LookAround;

            // Committed (awareness >= threshold): attack if in range, otherwise chase.
            if (detected && _distance <= attackRange)
                return StatuePhase.Attacking;

            return StatuePhase.Chasing;
        }

        // Phase entry: halt the previous phase's movement, announce, fire the wired event.
        private void Enter(UnityEvent stateEvent)
        {
            StopMovement();
            OnPhaseChanged?.Invoke(phase);
            stateEvent?.Invoke();
        }

        private void StopMovement()
        {
            if (chase.IsChasing) chase.StopChase();
            if (wander.IsWandering) wander.StopWander();
            motor.Stop();
        }

        private bool IsHome() =>
            Vector3.Distance(transform.position, _homePosition) <= baseArrivalDistance;

        // Flat look — keep the statue upright so the spawned attack fires level.
        private void FaceFlat(Vector3 worldPos)
        {
            worldPos.y = transform.position.y;
            if (worldPos != transform.position) transform.LookAt(worldPos);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            if (Application.isPlaying)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(_homePosition, baseArrivalDistance);
            }
        }
    }
}

// ============================================================================
// Setup on the statue prefab
//   1. NavMeshAgent + NavMeshMotor.
//   2. SightSensor — set requiredTags to ["Player"], tune range/fov/obstacle
//      layers. This is what wakes the statue.
//   3. Meter — initialValue 0, autoFill OFF. fillRate = how fast it wakes,
//      drainRate + drainDelay = how long it stays alert after losing the player.
//      (chaseThreshold here is independent of the Meter's own thresholds.)
//   4. NavMeshChase — autoPlay OFF (the controller starts it).
//   5. NavMeshWander — autoPlay OFF, radius = the search area for LookAround.
//   6. A Spawner aimed forward with the attack projectile prefab; its Cooldown
//      is the attack rate.
//   7. This StatueEnemyController. Wire all six module refs; set chaseThreshold,
//      attackRange, baseArrivalDistance. Wire per-phase UnityEvents (e.g. the
//      look-around animation on Triggered) as needed.
// ============================================================================
