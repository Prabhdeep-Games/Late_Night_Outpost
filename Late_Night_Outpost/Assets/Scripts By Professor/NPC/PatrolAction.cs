using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using System.Collections;

/// <summary>
/// PATROL ACTION
/// ====================
/// Makes an NPC walk between waypoints in a loop.
///
/// WHAT IT DOES:
/// - NPC moves from waypoint to waypoint
/// - Waits at each waypoint (optional)
/// - Loops back to the first waypoint when done
///
/// REQUIREMENTS:
/// - NavMeshAgent component on the same GameObject
/// - A baked NavMesh in your scene
/// - Waypoint GameObjects placed in the scene
///
/// HOW TO USE:
/// 1. Add this script to your NPC
/// 2. Create empty GameObjects as waypoints
/// 3. Drag waypoints into the "Waypoints" array
/// 4. Connect a Trigger to StartPatrol() or enable "Start On Awake"
///
/// TIPS:
/// - Create an empty parent called "Patrol Route" to organize waypoints
/// - Waypoints are shown as yellow spheres in Scene view
/// </summary>
public class PatrolAction : MonoBehaviour
{
    // ===== Target =====
    [Header("Target")]
    [Tooltip("The NavMeshAgent to control. If empty, uses this GameObject.")]
    public NavMeshAgent agent;

    // ===== Waypoints =====
    [Header("Waypoints")]
    [Tooltip("List of points the NPC will walk between.")]
    public Transform[] waypoints;

    // ===== Settings =====
    [Header("Settings")]
    [Tooltip("Start patrolling automatically when the game starts.")]
    public bool startOnAwake = false;

    [Tooltip("How long to wait at each waypoint (in seconds).")]
    public float waitTime = 1f;

    [Tooltip("How close the NPC needs to be to reach a waypoint.")]
    public float stoppingDistance = 0.5f;

    // ===== Events =====
    [Header("Events")]
    [Tooltip("Fired when patrol starts.")]
    public UnityEvent OnStartPatrol;

    [Tooltip("Fired when patrol stops.")]
    public UnityEvent OnStopPatrol;

    [Tooltip("Fired when NPC reaches a waypoint and starts waiting.")]
    public UnityEvent OnReachedWaypoint;

    [Tooltip("Fired when NPC finishes waiting and starts moving to next waypoint.")]
    public UnityEvent OnStartMovingToNext;

    // ===== State =====
    private int currentWaypointIndex = 0;
    private bool isPatrolling = false;
    private Coroutine patrolCoroutine;

    // ===== Initialization =====
    private void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        if (startOnAwake)
            StartPatrol();
    }

    // ===== Actions =====

    /// <summary>
    /// Start patrolling. Call this from a Trigger's UnityEvent.
    /// </summary>
    [ContextMenu("Start Patrol")]
    public void StartPatrol()
    {
        if (agent == null)
        {
            Debug.LogWarning("PatrolAction: No NavMeshAgent found!", this);
            return;
        }

        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogWarning("PatrolAction: No waypoints set!", this);
            return;
        }

        isPatrolling = true;
        agent.stoppingDistance = stoppingDistance;

        if (patrolCoroutine != null)
            StopCoroutine(patrolCoroutine);

        patrolCoroutine = StartCoroutine(PatrolLoop());
        OnStartPatrol?.Invoke();
    }

    /// <summary>
    /// Stop patrolling. The NPC will stop at its current position.
    /// </summary>
    [ContextMenu("Stop Patrol")]
    public void StopPatrol()
    {
        isPatrolling = false;

        if (patrolCoroutine != null)
        {
            StopCoroutine(patrolCoroutine);
            patrolCoroutine = null;
        }

        if (agent != null)
            agent.isStopped = true;

        OnStopPatrol?.Invoke();
    }

    // ===== Patrol Logic =====
    private IEnumerator PatrolLoop()
    {
        while (isPatrolling)
        {
            Transform targetWaypoint = waypoints[currentWaypointIndex];

            // Start moving
            agent.isStopped = false;
            agent.SetDestination(targetWaypoint.position);

            // Wait until we reach the waypoint
            while (isPatrolling && !HasReachedDestination())
            {
                yield return null;
            }

            // Reached waypoint
            if (isPatrolling)
            {
                OnReachedWaypoint?.Invoke();

                // Wait at waypoint
                if (waitTime > 0)
                {
                    agent.isStopped = true;
                    yield return new WaitForSeconds(waitTime);
                }

                // Move to next waypoint
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
                OnStartMovingToNext?.Invoke();
            }
        }
    }

    private bool HasReachedDestination()
    {
        if (agent.pathPending)
            return false;

        return agent.remainingDistance <= stoppingDistance;
    }

    // ===== Gizmos =====
    private void OnDrawGizmosSelected()
    {
        if (waypoints == null || waypoints.Length == 0)
            return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;

            Gizmos.DrawWireSphere(waypoints[i].position, 0.5f);

            int nextIndex = (i + 1) % waypoints.Length;
            if (waypoints[nextIndex] != null)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[nextIndex].position);
            }
        }

        if (Application.isPlaying && isPatrolling && currentWaypointIndex < waypoints.Length)
        {
            Gizmos.color = Color.green;
            if (waypoints[currentWaypointIndex] != null)
                Gizmos.DrawSphere(waypoints[currentWaypointIndex].position, 0.3f);
        }
    }
}

/*
 * ============================================
 * IMPLEMENTATION STEPS FOR STUDENTS
 * ============================================
 *
 * STEP 1: CREATE WAYPOINTS
 * - Create an empty GameObject called "Patrol Route"
 * - Create child empty GameObjects for each waypoint
 * - Position them where you want the NPC to walk
 * - Name them "Waypoint 1", "Waypoint 2", etc.
 *
 * STEP 2: SETUP THE NPC
 * - Add NavMeshAgent to your NPC
 * - Add this PatrolAction script
 * - Drag all waypoint objects into the "Waypoints" array
 *
 * STEP 3: CONFIGURE
 * - Set "Wait Time" for how long to pause at each point
 * - Enable "Start On Awake" for automatic patrol
 * - OR connect a trigger to StartPatrol()
 *
 * STEP 4: CONNECT ANIMATION EVENTS
 * - OnStartPatrol → Animator.SetBool("IsWalking", true)
 * - OnStopPatrol → Animator.SetBool("IsWalking", false)
 * - OnReachedWaypoint → Animator.SetTrigger("LookAround")
 * - OnStartMovingToNext → Animator.SetBool("IsWalking", true)
 *
 * STEP 5: TEST
 * - Press Play
 * - Watch your NPC walk the patrol route!
 * - Select the NPC to see waypoints in Scene view
 *
 * EXAMPLE: GUARD PATROL
 * - Create 4 waypoints around a building
 * - Set wait time to 2 seconds (guard looks around)
 * - Enable "Start On Awake"
 * - The guard will endlessly patrol!
 *
 * ============================================
 */
