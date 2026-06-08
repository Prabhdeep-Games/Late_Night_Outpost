using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using System.Collections;

/// <summary>
/// INVESTIGATE ACTION
/// ====================
/// Makes an NPC go investigate a location, then return home.
///
/// WHAT IT DOES:
/// - Remembers where the NPC was standing
/// - Walks to the investigation point
/// - Looks around for a moment
/// - Returns to the original position
/// - Fires events at each stage
///
/// REQUIREMENTS:
/// - NavMeshAgent component on the same GameObject
/// - A baked NavMesh in your scene
///
/// HOW TO USE:
/// 1. Add this script to your NPC
/// 2. Set investigation point OR call Investigate(position) dynamically
/// 3. Connect a Trigger to Investigate() method
/// 4. NPC will go check it out and come back!
///
/// TIPS:
/// - Great for stealth games ("What was that noise?")
/// - Use OnReachedTarget to play "looking around" animation
/// - Use OnReturnedHome to resume patrol
/// </summary>
public class InvestigateAction : MonoBehaviour
{
    // ===== Target =====
    [Header("Target")]
    [Tooltip("The NavMeshAgent to control. If empty, uses this GameObject.")]
    public NavMeshAgent agent;

    // ===== Investigation Point =====
    [Header("Investigation Point")]
    [Tooltip("Where to investigate. Can also be set dynamically via Investigate(position).")]
    public Transform investigateTarget;

    [Tooltip("If no target set, use this position.")]
    public Vector3 investigatePosition;

    // ===== Settings =====
    [Header("Settings")]
    [Tooltip("How long to wait at the investigation point (looking around).")]
    public float lookAroundTime = 3f;

    [Tooltip("How close to get before stopping.")]
    public float stoppingDistance = 0.5f;

    [Tooltip("Should the NPC return home after investigating?")]
    public bool returnHome = true;

    // ===== Events =====
    [Header("Events")]
    [Tooltip("Fired when the NPC starts moving to investigate.")]
    public UnityEvent OnStartInvestigating;

    [Tooltip("Fired when the NPC reaches the investigation point.")]
    public UnityEvent OnReachedTarget;

    [Tooltip("Fired when the NPC returns home.")]
    public UnityEvent OnReturnedHome;

    // ===== State =====
    private Vector3 homePosition;
    private bool isInvestigating = false;
    private Coroutine investigateCoroutine;

    // ===== Initialization =====
    private void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        // Remember starting position as home
        homePosition = transform.position;
    }

    // ===== Actions =====

    /// <summary>
    /// Start investigating. Call this from a Trigger's UnityEvent.
    /// </summary>
    [ContextMenu("Investigate")]
    public void Investigate()
    {
        if (agent == null)
        {
            Debug.LogWarning("InvestigateAction: No NavMeshAgent found!", this);
            return;
        }

        // Determine target position
        Vector3 targetPos = investigateTarget != null
            ? investigateTarget.position
            : investigatePosition;

        StartInvestigation(targetPos);
    }

    /// <summary>
    /// Investigate a specific position. Great for dynamic events like sounds.
    /// </summary>
    public void Investigate(Vector3 position)
    {
        investigatePosition = position;
        investigateTarget = null;
        Investigate();
    }

    /// <summary>
    /// Investigate a specific transform.
    /// </summary>
    public void Investigate(Transform target)
    {
        investigateTarget = target;
        Investigate();
    }

    /// <summary>
    /// Cancel investigation and stop immediately.
    /// </summary>
    [ContextMenu("Cancel")]
    public void Cancel()
    {
        isInvestigating = false;

        if (investigateCoroutine != null)
        {
            StopCoroutine(investigateCoroutine);
            investigateCoroutine = null;
        }

        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    /// <summary>
    /// Update the home position to current position.
    /// </summary>
    public void SetCurrentAsHome()
    {
        homePosition = transform.position;
    }

    // ===== Investigation Logic =====
    private void StartInvestigation(Vector3 targetPos)
    {
        // Remember current position as home (if not already investigating)
        if (!isInvestigating)
        {
            homePosition = transform.position;
        }

        // Stop any existing investigation
        if (investigateCoroutine != null)
            StopCoroutine(investigateCoroutine);

        isInvestigating = true;
        investigateCoroutine = StartCoroutine(InvestigateRoutine(targetPos));
    }

    private IEnumerator InvestigateRoutine(Vector3 targetPos)
    {
        // === PHASE 1: Go to investigation point ===
        OnStartInvestigating?.Invoke();

        agent.isStopped = false;
        agent.stoppingDistance = stoppingDistance;
        agent.SetDestination(targetPos);

        // Wait until we arrive
        while (!HasReachedDestination())
        {
            yield return null;
        }

        // === PHASE 2: Look around ===
        OnReachedTarget?.Invoke();

        agent.isStopped = true;
        yield return new WaitForSeconds(lookAroundTime);

        // === PHASE 3: Return home ===
        if (returnHome)
        {
            agent.isStopped = false;
            agent.SetDestination(homePosition);

            // Wait until we arrive home
            while (!HasReachedDestination())
            {
                yield return null;
            }

            agent.isStopped = true;
            OnReturnedHome?.Invoke();
        }

        isInvestigating = false;
        investigateCoroutine = null;
    }

    private bool HasReachedDestination()
    {
        if (agent.pathPending)
            return false;

        return agent.remainingDistance <= stoppingDistance;
    }

    /// <summary>
    /// Check if currently investigating.
    /// </summary>
    public bool IsInvestigating()
    {
        return isInvestigating;
    }

    // ===== Gizmos =====
    private void OnDrawGizmosSelected()
    {
        // Show home position (blue)
        Vector3 home = Application.isPlaying ? homePosition : transform.position;
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(home, Vector3.one * 0.5f);

        // Show investigation target (yellow)
        Vector3 targetPos = investigateTarget != null
            ? investigateTarget.position
            : investigatePosition;

        if (targetPos != Vector3.zero || investigateTarget != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(targetPos, 0.5f);
            Gizmos.DrawLine(home, targetPos);

            // Question mark indicator
            Gizmos.DrawWireSphere(targetPos + Vector3.up * 1.5f, 0.2f);
        }

        // Show stopping distance
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawSphere(targetPos, stoppingDistance);
    }
}

/*
 * ============================================
 * IMPLEMENTATION STEPS FOR STUDENTS
 * ============================================
 *
 * STEP 1: SETUP THE NPC
 * - Add NavMeshAgent to your NPC (e.g., a guard)
 * - Add this InvestigateAction script
 *
 * STEP 2: SET INVESTIGATION POINT
 * Option A: Fixed point
 *   - Create an empty GameObject where the "noise" is
 *   - Drag it into "Investigate Target"
 *
 * Option B: Dynamic point
 *   - Call Investigate(position) from code
 *   - E.g., when player makes noise, pass that position
 *
 * STEP 3: CONNECT A TRIGGER
 * - Add a trigger (e.g., player steps on noisy floor)
 * - Connect the event to Investigate()
 * - Guard will go check it out!
 *
 * STEP 4: ADD BEHAVIORS (OPTIONAL)
 * - OnStartInvestigating → Play alert animation ("Huh?")
 * - OnReachedTarget → Play look around animation
 * - OnReturnedHome → Resume patrol (PatrolAction.StartPatrol)
 *
 * ============================================
 * EXAMPLE: STEALTH GAME GUARD
 * ============================================
 *
 * Setup:
 * - Guard has: PatrolAction + InvestigateAction
 * - Player has a "MakeNoise" trigger
 *
 * Flow:
 * 1. Guard is patrolling (PatrolAction)
 * 2. Player makes noise → Investigate(noisePosition)
 * 3. Guard stops patrol, goes to investigate
 * 4. Guard looks around for 3 seconds
 * 5. Guard returns home
 * 6. OnReturnedHome → PatrolAction.StartPatrol()
 * 7. Guard resumes patrol!
 *
 * ============================================
 * EXAMPLE: CURIOUS ANIMAL
 * ============================================
 *
 * Setup:
 * - Deer has: WanderAction + InvestigateAction
 * - Place food objects with TriggerZone
 *
 * Flow:
 * 1. Deer is wandering around
 * 2. Deer enters food trigger zone
 * 3. TriggerZone calls Investigate(food.position)
 * 4. Deer walks to food, "sniffs" for 3 seconds
 * 5. Deer returns to wandering area
 * 6. OnReturnedHome → WanderAction.StartWander()
 *
 * ============================================
 */
