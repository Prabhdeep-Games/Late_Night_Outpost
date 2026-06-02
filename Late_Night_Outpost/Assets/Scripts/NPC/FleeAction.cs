using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

/// <summary>
/// FLEE ACTION
/// ====================
/// Makes an NPC run away from a target.
///
/// WHAT IT DOES:
/// - NPC moves in the opposite direction of the target
/// - Keeps running until it reaches a safe distance
/// - Fires an event when safe
///
/// REQUIREMENTS:
/// - NavMeshAgent component on the same GameObject
/// - A baked NavMesh in your scene
///
/// HOW TO USE:
/// 1. Add this script to your NPC
/// 2. Set "Target" to what the NPC should flee from (e.g., player)
/// 3. Connect a Trigger to StartFlee() to begin fleeing
///
/// TIPS:
/// - Great for scared NPCs, prey animals, civilians
/// - Combine with TriggerZone for "fear range" detection
/// - Opposite of ChaseAction!
/// </summary>
public class FleeAction : MonoBehaviour
{
    // ===== Target =====
    [Header("Target")]
    [Tooltip("The NavMeshAgent to control. If empty, uses this GameObject.")]
    public NavMeshAgent agent;

    [Tooltip("The target to flee from (e.g., the Player).")]
    public Transform target;

    // ===== Settings =====
    [Header("Settings")]
    [Tooltip("How far to run away each time.")]
    public float fleeDistance = 10f;

    [Tooltip("Safe distance - stop fleeing when this far from target.")]
    public float safeDistance = 15f;

    [Tooltip("How close to destination before picking a new flee point.")]
    public float stoppingDistance = 1f;

    [Tooltip("How often to recalculate flee direction (in seconds).")]
    public float updateRate = 0.5f;

    // ===== Events =====
    [Header("Events")]
    [Tooltip("Fired when the NPC reaches a safe distance.")]
    public UnityEvent OnReachedSafety;

    // ===== State =====
    private bool isFleeing = false;
    private float nextUpdateTime = 0f;
    private bool hasReachedSafety = false;

    // ===== Initialization =====
    private void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();
    }

    // ===== Update =====
    private void Update()
    {
        if (!isFleeing || target == null || agent == null)
            return;

        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        // Check if we're safe
        if (!hasReachedSafety && distanceToTarget >= safeDistance)
        {
            hasReachedSafety = true;
            OnReachedSafety?.Invoke();
        }
        else if (hasReachedSafety && distanceToTarget < safeDistance)
        {
            // Target got closer, no longer safe
            hasReachedSafety = false;
        }

        // Keep fleeing if not safe
        if (!hasReachedSafety && Time.time >= nextUpdateTime)
        {
            FleeFromTarget();
            nextUpdateTime = Time.time + updateRate;
        }
    }

    // ===== Actions =====

    /// <summary>
    /// Start fleeing from the target. Call this from a Trigger's UnityEvent.
    /// </summary>
    [ContextMenu("Start Flee")]
    public void StartFlee()
    {
        if (agent == null)
        {
            Debug.LogWarning("FleeAction: No NavMeshAgent found!", this);
            return;
        }

        if (target == null)
        {
            Debug.LogWarning("FleeAction: No target set!", this);
            return;
        }

        isFleeing = true;
        hasReachedSafety = false;
        agent.isStopped = false;
        agent.stoppingDistance = stoppingDistance;
        FleeFromTarget();
    }

    /// <summary>
    /// Start fleeing from a specific target.
    /// </summary>
    public void StartFlee(Transform fleeFrom)
    {
        target = fleeFrom;
        StartFlee();
    }

    /// <summary>
    /// Stop fleeing. The NPC will stop moving.
    /// </summary>
    [ContextMenu("Stop Flee")]
    public void StopFlee()
    {
        isFleeing = false;

        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    // ===== Flee Logic =====
    private void FleeFromTarget()
    {
        // Calculate direction away from target
        Vector3 fleeDirection = (transform.position - target.position).normalized;
        Vector3 fleePosition = transform.position + fleeDirection * fleeDistance;

        // Find valid point on NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(fleePosition, out hit, fleeDistance, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    /// <summary>
    /// Check if currently fleeing.
    /// </summary>
    public bool IsFleeing()
    {
        return isFleeing;
    }

    // ===== Gizmos =====
    private void OnDrawGizmosSelected()
    {
        // Show safe distance (green = safe zone)
        Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
        Gizmos.DrawSphere(transform.position, safeDistance);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, safeDistance);

        // Show flee distance (yellow = how far to run)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, fleeDistance);

        // Show line to target (red = danger!)
        if (target != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, target.position);
            Gizmos.DrawWireSphere(target.position, 0.5f);
        }
    }
}

/*
 * ============================================
 * IMPLEMENTATION STEPS FOR STUDENTS
 * ============================================
 *
 * STEP 1: SETUP THE NPC
 * - Add NavMeshAgent to your NPC
 * - Add this FleeAction script
 * - Drag the Player (or threat) into the "Target" field
 *
 * STEP 2: CREATE A FEAR TRIGGER
 * - Add a Sphere Collider to the NPC (set as Trigger)
 * - Add a TriggerZone script
 * - Set the filter tag to "Player"
 * - Connect EnterEvent to StartFlee()
 *
 * STEP 3: CONFIGURE DISTANCES
 * - "Flee Distance" = how far to run each time (e.g., 10)
 * - "Safe Distance" = when to stop fleeing (e.g., 15)
 * - Make safe > flee so NPC actually escapes!
 *
 * STEP 4: ADD SAFETY BEHAVIOR (OPTIONAL)
 * - In "On Reached Safety" event, connect actions like:
 *   - Return to patrol (PatrolAction.StartPatrol)
 *   - Play relieved animation
 *   - Stop fleeing (StopFlee)
 *
 * EXAMPLE: SCARED VILLAGER
 * - Villager has: NavMeshAgent + FleeAction + TriggerZone
 * - When player with "Enemy" tag enters → StartFlee()
 * - On reached safety → start wandering again
 *
 * EXAMPLE: PREY ANIMAL
 * - Deer has FleeAction, target = Player
 * - Large detection trigger
 * - Flee distance = 20, Safe distance = 30
 * - Deer runs far away when player approaches!
 *
 * ============================================
 */
