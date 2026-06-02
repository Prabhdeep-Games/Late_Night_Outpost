using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

/// <summary>
/// CHASE ACTION
/// ====================
/// Makes an NPC chase/follow a target using NavMesh.
///
/// WHAT IT DOES:
/// - NPC continuously moves towards a target (e.g., the player)
/// - Updates path as the target moves
/// - Fires an event when reaching the target
///
/// REQUIREMENTS:
/// - NavMeshAgent component on the same GameObject
/// - A baked NavMesh in your scene
///
/// HOW TO USE:
/// 1. Add this script to your NPC
/// 2. Set "Target" to the player or object to chase
/// 3. Connect a Trigger to StartChase() to begin chasing
/// 4. Optionally connect "On Reached Target" to do something on catch
///
/// TIPS:
/// - Use with TriggerZone for "aggro range" detection
/// - Combine with PatrolAction for patrol-then-chase behavior
/// </summary>
public class ChaseAction : MonoBehaviour
{
    // ===== Target =====
    [Header("Target")]
    [Tooltip("The NavMeshAgent to control. If empty, uses this GameObject.")]
    public NavMeshAgent agent;

    [Tooltip("The target to chase (e.g., the Player).")]
    public Transform target;

    // ===== Settings =====
    [Header("Settings")]
    [Tooltip("How close to get before stopping.")]
    public float stoppingDistance = 1.5f;

    [Tooltip("How often to update the path (in seconds). Lower = smoother but costs more.")]
    public float updateRate = 0.2f;

    // ===== Events =====
    [Header("Events")]
    [Tooltip("Fired when chase starts.")]
    public UnityEvent OnStartChase;

    [Tooltip("Fired when chase stops.")]
    public UnityEvent OnStopChase;

    [Tooltip("Fired when the NPC reaches the target.")]
    public UnityEvent OnReachedTarget;

    // ===== State =====
    private bool isChasing = false;
    private float nextUpdateTime = 0f;
    private bool hasReachedTarget = false;

    // ===== Initialization =====
    private void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();
    }

    // ===== Update =====
    private void Update()
    {
        if (!isChasing || target == null || agent == null)
            return;

        // Update path periodically
        if (Time.time >= nextUpdateTime)
        {
            agent.SetDestination(target.position);
            nextUpdateTime = Time.time + updateRate;
        }

        // Check if we reached the target
        if (!hasReachedTarget && HasReachedTarget())
        {
            hasReachedTarget = true;
            OnReachedTarget?.Invoke();
        }
        else if (hasReachedTarget && !HasReachedTarget())
        {
            // Target moved away, reset flag
            hasReachedTarget = false;
        }
    }

    // ===== Actions =====

    /// <summary>
    /// Start chasing the target. Call this from a Trigger's UnityEvent.
    /// </summary>
    [ContextMenu("Start Chase")]
    public void StartChase()
    {
        if (agent == null)
        {
            Debug.LogWarning("ChaseAction: No NavMeshAgent found!", this);
            return;
        }

        if (target == null)
        {
            Debug.LogWarning("ChaseAction: No target set!", this);
            return;
        }

        isChasing = true;
        hasReachedTarget = false;
        agent.isStopped = false;
        agent.stoppingDistance = stoppingDistance;
        agent.SetDestination(target.position);

        OnStartChase?.Invoke();
    }

    /// <summary>
    /// Start chasing a specific target.
    /// </summary>
    public void StartChase(Transform newTarget)
    {
        target = newTarget;
        StartChase();
    }

    /// <summary>
    /// Stop chasing. The NPC will stop moving.
    /// </summary>
    [ContextMenu("Stop Chase")]
    public void StopChase()
    {
        isChasing = false;

        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    // ===== Helpers =====
    private bool HasReachedTarget()
    {
        if (target == null) return false;

        float distance = Vector3.Distance(transform.position, target.position);
        return distance <= stoppingDistance;
    }

    /// <summary>
    /// Check if currently chasing.
    /// </summary>
    public bool IsChasing()
    {
        return isChasing;
    }

    // ===== Gizmos =====
    private void OnDrawGizmosSelected()
    {
        // Show stopping distance
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stoppingDistance);

        // Show line to target
        if (target != null)
        {
            Gizmos.color = isChasing ? Color.red : Color.gray;
            Gizmos.DrawLine(transform.position, target.position);
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
 * - Add this ChaseAction script
 * - Drag the Player (or target) into the "Target" field
 *
 * STEP 2: CREATE AN AGGRO TRIGGER
 * - Add a large Sphere Collider to the NPC (set as Trigger)
 * - Add a TriggerZone script
 * - Set the filter tag to "Player"
 * - Connect EnterEvent to StartChase()
 * - Connect ExitEvent to StopChase() (optional)
 *
 * STEP 3: CONFIGURE
 * - Set "Stopping Distance" (how close before stopping)
 * - Adjust "Update Rate" if needed (0.2 is good default)
 *
 * STEP 4: ADD CATCH BEHAVIOR (OPTIONAL)
 * - In "On Reached Target" event, connect actions like:
 *   - Play attack animation
 *   - Deal damage to player
 *   - Play sound effect
 *
 * EXAMPLE: ENEMY THAT CHASES PLAYER
 * - NPC has: NavMeshAgent + ChaseAction + TriggerZone
 * - Large trigger collider as "detection range"
 * - When player enters → StartChase()
 * - When player exits → StopChase()
 * - On reached → Attack!
 *
 * ============================================
 */
