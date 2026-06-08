using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.Events;

/// <summary>
/// WANDER ACTION
/// ====================
/// Makes an NPC wander randomly within an area.
///
/// WHAT IT DOES:
/// - NPC picks a random point nearby
/// - Walks to that point
/// - Waits, then picks another random point
/// - Repeats forever (until stopped)
///
/// REQUIREMENTS:
/// - NavMeshAgent component on the same GameObject
/// - A baked NavMesh in your scene
///
/// HOW TO USE:
/// 1. Add this script to your NPC
/// 2. Set the "Wander Radius" (how far it can go)
/// 3. Connect a Trigger to StartWander() or enable "Start On Awake"
///
/// TIPS:
/// - Great for ambient NPCs, animals, villagers
/// - The NPC stays within the radius of its starting position
/// - Blue circle in Scene view shows wander area
/// </summary>
public class WanderAction : MonoBehaviour
{
    // ===== Target =====
    [Header("Target")]
    [Tooltip("The NavMeshAgent to control. If empty, uses this GameObject.")]
    public NavMeshAgent agent;

    // ===== Settings =====
    [Header("Settings")]
    [Tooltip("Start wandering automatically when the game starts.")]
    public bool startOnAwake = false;

    [Tooltip("Maximum distance from starting point the NPC can wander.")]
    public float wanderRadius = 10f;

    [Tooltip("How long to wait at each destination before moving again.")]
    public float waitTime = 2f;

    [Tooltip("Minimum wait time (for random variation).")]
    public float waitTimeMin = 1f;

    [Tooltip("How close to destination before considering it reached.")]
    public float stoppingDistance = 0.5f;
    
    public UnityEvent OnWanderStart;
    public UnityEvent OnWanderEnd;

    // ===== State =====
    private Vector3 startPosition;
    private bool isWandering = false;
    private Coroutine wanderCoroutine;

    // ===== Initialization =====
    private void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        // Remember starting position
        startPosition = transform.position;
    }

    private void Start()
    {
        if (startOnAwake)
            StartWander();
    }

    // ===== Actions =====

    /// <summary>
    /// Start wandering. Call this from a Trigger's UnityEvent.
    /// </summary>
    [ContextMenu("Start Wander")]
    public void StartWander()
    {
        if (agent == null)
        {
            Debug.LogWarning("WanderAction: No NavMeshAgent found!", this);
            return;
        }

        isWandering = true;
        agent.stoppingDistance = stoppingDistance;

        // Stop any existing wander
        if (wanderCoroutine != null)
            StopCoroutine(wanderCoroutine);

        // Start wandering
        wanderCoroutine = StartCoroutine(WanderLoop());
        
        OnWanderStart.Invoke();
    }

    /// <summary>
    /// Stop wandering. The NPC will stop at its current position.
    /// </summary>
    [ContextMenu("Stop Wander")]
    public void StopWander()
    {
        isWandering = false;

        if (wanderCoroutine != null)
        {
            StopCoroutine(wanderCoroutine);
            wanderCoroutine = null;
        }

        if (agent != null)
            agent.isStopped = true;
        
        OnWanderEnd.Invoke();
    }

    // ===== Wander Logic =====
    private IEnumerator WanderLoop()
    {
        while (isWandering)
        {
            // Pick a random destination
            Vector3 randomPoint = GetRandomPoint();

            // Move to it
            agent.isStopped = false;
            agent.SetDestination(randomPoint);

            // Wait until we arrive
            while (isWandering && !HasReachedDestination())
            {
                yield return null;
            }

            // Wait at destination
            if (isWandering)
            {
                agent.isStopped = true;
                float randomWait = Random.Range(waitTimeMin, waitTime);
                yield return new WaitForSeconds(randomWait);
            }
        }
    }

    private Vector3 GetRandomPoint()
    {
        // Try to find a valid point on the NavMesh
        for (int i = 0; i < 30; i++)
        {
            // Random point in circle
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection.y = 0; // Keep on same height
            Vector3 randomPoint = startPosition + randomDirection;

            // Check if it's on the NavMesh
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 2f, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }

        // Fallback: return current position
        return transform.position;
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
        // Show wander area
        Vector3 center = Application.isPlaying ? startPosition : transform.position;

        Gizmos.color = new Color(0.3f, 0.5f, 1f, 0.3f);
        Gizmos.DrawSphere(center, wanderRadius);

        Gizmos.color = new Color(0.3f, 0.5f, 1f, 1f);
        Gizmos.DrawWireSphere(center, wanderRadius);
    }
}

/*
 * ============================================
 * IMPLEMENTATION STEPS FOR STUDENTS
 * ============================================
 *
 * STEP 1: SETUP THE NPC
 * - Create a GameObject for your NPC
 * - Add NavMeshAgent component
 * - Add this WanderAction script
 *
 * STEP 2: CONFIGURE WANDER AREA
 * - Set "Wander Radius" (e.g., 10 for a small area)
 * - Select the NPC to see the blue circle in Scene view
 * - The NPC will stay within this circle
 *
 * STEP 3: CONFIGURE TIMING
 * - "Wait Time" = max time to pause (e.g., 3 seconds)
 * - "Wait Time Min" = min time to pause (e.g., 1 second)
 * - NPC waits a random time between min and max
 *
 * STEP 4: START WANDERING
 * - Enable "Start On Awake" for automatic wandering
 * - OR connect a trigger to StartWander()
 *
 * STEP 5: TEST
 * - Press Play
 * - Watch your NPC wander around!
 *
 * EXAMPLE: VILLAGE NPC
 * - Place NPC in the village center
 * - Set wander radius to 15
 * - Set wait time 2-5 seconds
 * - The villager will roam around naturally!
 *
 * EXAMPLE: ANIMAL IN A PEN
 * - Place animal in the pen center
 * - Set wander radius to match pen size
 * - Animal stays in its area
 *
 * ============================================
 */
