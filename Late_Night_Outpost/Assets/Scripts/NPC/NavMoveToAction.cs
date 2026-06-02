using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

/// <summary>
/// NAV MOVE TO ACTION
/// ====================
/// Makes an NPC walk to a target position using Unity's Navigation system.
///
/// WHAT IT DOES:
/// - Moves the NPC to a destination using NavMesh pathfinding
/// - The NPC will automatically avoid obstacles
/// - Can move to a Transform target or a specific world position
///
/// REQUIREMENTS:
/// - NavMeshAgent component on the same GameObject
/// - A baked NavMesh in your scene (Window > AI > Navigation)
///
/// HOW TO USE:
/// 1. Add this script to your NPC (must have NavMeshAgent)
/// 2. Set the "Destination" to where you want the NPC to go
/// 3. Connect a Trigger's UnityEvent to the MoveTo() method
/// 4. When triggered, the NPC walks to the destination
///
/// TIPS:
/// - Leave "Destination" empty and set "Destination Position" for fixed points
/// - Use the right-click context menu to test in Play mode
/// </summary>
public class NavMoveToAction : MonoBehaviour
{
    // ===== Target =====
    [Header("Target")]
    [Tooltip("The NavMeshAgent to control. If empty, uses this GameObject.")]
    public NavMeshAgent agent;

    // ===== Destination =====
    [Header("Destination")]
    [Tooltip("Transform to move towards. If set, overrides Destination Position.")]
    public Transform destination;

    [Tooltip("Fixed world position to move to. Used if Destination is not set.")]
    public Vector3 destinationPosition;

    // ===== Settings =====
    [Header("Settings")]
    [Tooltip("How close the NPC needs to get to the destination.")]
    public float stoppingDistance = 0.5f;

    // ===== Events =====
    [Header("Events")]
    [Tooltip("Fired when NPC starts moving to destination.")]
    public UnityEvent OnStartMoving;

    [Tooltip("Fired when NPC reaches the destination.")]
    public UnityEvent OnReachedDestination;

    // ===== State =====
    private bool isMoving = false;

    // ===== Initialization =====
    private void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();
    }

    // ===== Update =====
    private void Update()
    {
        if (!isMoving || agent == null)
            return;

        // Check if we reached destination
        if (HasReachedDestination())
        {
            isMoving = false;
            OnReachedDestination?.Invoke();
        }
    }

    // ===== Actions =====

    /// <summary>
    /// Move to the destination. Call this from a Trigger's UnityEvent.
    /// </summary>
    [ContextMenu("Move To Destination")]
    public void MoveTo()
    {
        if (agent == null)
        {
            Debug.LogWarning("NavMoveToAction: No NavMeshAgent found!", this);
            return;
        }

        agent.stoppingDistance = stoppingDistance;
        agent.isStopped = false;

        Vector3 targetPos = destination != null ? destination.position : destinationPosition;
        agent.SetDestination(targetPos);

        isMoving = true;
        OnStartMoving?.Invoke();
    }

    /// <summary>
    /// Move to a specific Transform. Useful for dynamic targets.
    /// </summary>
    public void MoveTo(Transform target)
    {
        if (target != null)
        {
            destination = target;
            MoveTo();
        }
    }

    /// <summary>
    /// Stop moving immediately.
    /// </summary>
    [ContextMenu("Stop")]
    public void Stop()
    {
        isMoving = false;
        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    // ===== Helpers =====
    private bool HasReachedDestination()
    {
        if (agent.pathPending)
            return false;

        return agent.remainingDistance <= stoppingDistance;
    }

    // ===== Gizmos =====
    private void OnDrawGizmosSelected()
    {
        Vector3 targetPos = destination != null ? destination.position : destinationPosition;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(targetPos, 0.5f);
        Gizmos.DrawLine(transform.position, targetPos);
    }
}

/*
 * ============================================
 * IMPLEMENTATION STEPS FOR STUDENTS
 * ============================================
 *
 * STEP 1: SETUP YOUR NPC
 * - Create a GameObject for your NPC (e.g., a Capsule)
 * - Add Component > Navigation > NavMeshAgent
 * - Add this script (NavMoveToAction)
 *
 * STEP 2: BAKE THE NAVMESH
 * - Go to Window > AI > Navigation
 * - Select your ground/floor objects
 * - Mark them as "Navigation Static"
 * - Click "Bake" to create the NavMesh
 *
 * STEP 3: SET THE DESTINATION
 * - Create an empty GameObject where you want the NPC to go
 * - Drag it into the "Destination" field
 * - OR just set "Destination Position" coordinates
 *
 * STEP 4: CONNECT A TRIGGER
 * - Add a trigger (e.g., KeyTrigger, TriggerZone)
 * - In the trigger's event, click "+"
 * - Drag your NPC into the object field
 * - Select NavMoveToAction > MoveTo()
 *
 * STEP 5: CONNECT ANIMATION EVENTS
 * - OnStartMoving → Animator.SetBool("IsWalking", true)
 * - OnReachedDestination → Animator.SetBool("IsWalking", false)
 *
 * STEP 6: TEST
 * - Press Play
 * - Activate your trigger
 * - Watch the NPC walk to the destination!
 *
 * ============================================
 */
