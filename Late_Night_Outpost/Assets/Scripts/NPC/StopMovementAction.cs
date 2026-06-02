using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

/// <summary>
/// STOP MOVEMENT ACTION
/// ====================
/// Stops an NPC from moving immediately.
///
/// WHAT IT DOES:
/// - Stops the NavMeshAgent instantly
/// - Can optionally resume movement later
///
/// REQUIREMENTS:
/// - NavMeshAgent component on the same GameObject
///
/// HOW TO USE:
/// 1. Add this script to your NPC (must have NavMeshAgent)
/// 2. Connect a Trigger's UnityEvent to Stop() method
/// 3. When triggered, the NPC stops walking
///
/// TIPS:
/// - Great for dialogue, cutscenes, or stun effects
/// - Use Resume() to continue to the previous destination
/// </summary>
public class StopMovementAction : MonoBehaviour
{
    // ===== Target =====
    [Header("Target")]
    [Tooltip("The NavMeshAgent to control. If empty, uses this GameObject.")]
    public NavMeshAgent agent;

    // ===== Settings =====
    [Header("Settings")]
    [Tooltip("If true, clears the destination. If false, just pauses movement.")]
    public bool clearDestination = false;

    // ===== Events =====
    [Header("Events")]
    [Tooltip("Fired when NPC stops moving.")]
    public UnityEvent OnStopped;

    [Tooltip("Fired when NPC resumes moving.")]
    public UnityEvent OnResumed;

    // ===== Initialization =====
    private void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();
    }

    // ===== Actions =====

    /// <summary>
    /// Stop the NPC from moving. Call this from a Trigger's UnityEvent.
    /// </summary>
    [ContextMenu("Stop")]
    public void Stop()
    {
        if (agent == null)
        {
            Debug.LogWarning("StopMovementAction: No NavMeshAgent found!", this);
            return;
        }

        agent.isStopped = true;

        if (clearDestination)
        {
            agent.ResetPath();
        }

        OnStopped?.Invoke();
    }

    /// <summary>
    /// Resume movement to the previous destination.
    /// </summary>
    [ContextMenu("Resume")]
    public void Resume()
    {
        if (agent == null)
        {
            Debug.LogWarning("StopMovementAction: No NavMeshAgent found!", this);
            return;
        }

        agent.isStopped = false;
        OnResumed?.Invoke();
    }
}

/*
 * ============================================
 * IMPLEMENTATION STEPS FOR STUDENTS
 * ============================================
 *
 * STEP 1: SETUP
 * - Add this script to an NPC that has a NavMeshAgent
 *
 * STEP 2: CONNECT TRIGGERS
 * - Add a trigger (e.g., TriggerZone for a "stop zone")
 * - Connect the trigger's event to Stop()
 * - Optionally connect another trigger to Resume()
 *
 * STEP 3: CONNECT ANIMATION EVENTS
 * - OnStopped → Animator.SetBool("IsWalking", false)
 * - OnResumed → Animator.SetBool("IsWalking", true)
 *
 * STEP 4: TEST
 * - Start the NPC moving (with NavMoveToAction or PatrolAction)
 * - Trigger the stop
 * - The NPC freezes in place!
 *
 * EXAMPLE USE CASES:
 * - Player enters dialogue range → NPC stops and talks
 * - NPC gets hit → stunned, stops moving
 * - Cutscene trigger → all NPCs freeze
 *
 * ============================================
 */
