using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// AWARENESS METER
/// ====================
/// Adds gradual detection to NPCs - like stealth games!
///
/// WHAT IT DOES:
/// - Awareness fills up while target is visible
/// - Awareness drains when target is hidden
/// - Fires events at different awareness levels
/// - Creates tension: "Am I about to be seen?!"
///
/// REQUIREMENTS:
/// - SightDetection component on the same GameObject
/// - (Or manually call SetVisible from your own detection)
///
/// HOW TO USE:
/// 1. Add SightDetection to your NPC first
/// 2. Add this AwarenessMeter script
/// 3. Connect events to NPC behaviors
/// 4. Watch the meter fill and drain!
///
/// TIPS:
/// - Use OnSuspicious for "Huh?" reactions (not full alert yet)
/// - Use OnFullyDetected for actual chase/combat
/// - Adjust fill/drain speed to control difficulty
/// </summary>
public class AwarenessMeter : MonoBehaviour
{
    // ===== Source =====
    [Header("Detection Source")]
    [Tooltip("The SightDetection to read from. If empty, tries to find one on this GameObject.")]
    public SightDetection sightDetection;

    // ===== Meter Settings =====
    [Header("Meter Settings")]
    [Tooltip("How fast awareness fills when target is visible (per second).")]
    public float fillSpeed = 0.5f;

    [Tooltip("How fast awareness drains when target is hidden (per second).")]
    public float drainSpeed = 0.3f;

    [Tooltip("Delay before awareness starts draining after losing sight.")]
    public float drainDelay = 1f;

    // ===== Thresholds =====
    [Header("Thresholds")]
    [Tooltip("Awareness level to trigger 'suspicious' state (0-1).")]
    [Range(0f, 1f)]
    public float suspiciousThreshold = 0.3f;

    [Tooltip("Awareness level to trigger 'fully detected' state (0-1).")]
    [Range(0f, 1f)]
    public float detectedThreshold = 1f;

    // ===== Events =====
    [Header("Events")]
    [Tooltip("Fired when awareness crosses the suspicious threshold (going up).")]
    public UnityEvent OnSuspicious;

    [Tooltip("Fired when awareness reaches full detection.")]
    public UnityEvent OnFullyDetected;

    [Tooltip("Fired when awareness drops back to zero.")]
    public UnityEvent OnAwarenessLost;

    [Tooltip("Fired every frame with current awareness value (0-1). Use for UI.")]
    public UnityEvent<float> OnAwarenessChanged;

    // ===== State =====
    [Header("Debug (Read Only)")]
    [SerializeField] private float awareness = 0f;
    [SerializeField] private AwarenessState currentState = AwarenessState.Unaware;

    private float timeSinceLostSight = 0f;
    private bool isVisible = false;

    public enum AwarenessState
    {
        Unaware,    // awareness = 0
        Suspicious, // awareness > suspiciousThreshold
        Detected    // awareness >= detectedThreshold
    }

    // ===== Public Properties =====
    /// <summary>
    /// Current awareness level (0 to 1).
    /// </summary>
    public float Awareness => awareness;

    /// <summary>
    /// Current awareness state.
    /// </summary>
    public AwarenessState CurrentState => currentState;

    /// <summary>
    /// Is the NPC fully alerted?
    /// </summary>
    public bool IsDetected => currentState == AwarenessState.Detected;

    /// <summary>
    /// Is the NPC suspicious but not fully alerted?
    /// </summary>
    public bool IsSuspicious => currentState == AwarenessState.Suspicious;

    // ===== Initialization =====
    private void Awake()
    {
        if (sightDetection == null)
            sightDetection = GetComponent<SightDetection>();
    }

    // ===== Update =====
    private void Update()
    {
        // Get visibility from SightDetection
        if (sightDetection != null)
        {
            isVisible = sightDetection.CanSeeTarget;
        }

        // Update awareness meter
        UpdateAwareness();

        // Update state and fire events
        UpdateState();
    }

    private void UpdateAwareness()
    {
        if (isVisible)
        {
            // Target visible - fill meter
            awareness += fillSpeed * Time.deltaTime;
            awareness = Mathf.Min(awareness, 1f);
            timeSinceLostSight = 0f;
        }
        else
        {
            // Target not visible - drain meter (after delay)
            timeSinceLostSight += Time.deltaTime;

            if (timeSinceLostSight >= drainDelay)
            {
                awareness -= drainSpeed * Time.deltaTime;
                awareness = Mathf.Max(awareness, 0f);
            }
        }

        // Notify listeners of change
        OnAwarenessChanged?.Invoke(awareness);
    }

    private void UpdateState()
    {
        AwarenessState newState;

        if (awareness >= detectedThreshold)
            newState = AwarenessState.Detected;
        else if (awareness >= suspiciousThreshold)
            newState = AwarenessState.Suspicious;
        else
            newState = AwarenessState.Unaware;

        // Fire events on state change
        if (newState != currentState)
        {
            AwarenessState oldState = currentState;
            currentState = newState;

            // Going UP in awareness
            if (newState == AwarenessState.Suspicious && oldState == AwarenessState.Unaware)
            {
                OnSuspicious?.Invoke();
            }
            else if (newState == AwarenessState.Detected)
            {
                OnFullyDetected?.Invoke();
            }

            // Going DOWN in awareness
            if (newState == AwarenessState.Unaware && oldState != AwarenessState.Unaware)
            {
                OnAwarenessLost?.Invoke();
            }
        }
    }

    // ===== Public Methods =====

    /// <summary>
    /// Manually set visibility (if not using SightDetection).
    /// </summary>
    public void SetVisible(bool visible)
    {
        isVisible = visible;
    }

    /// <summary>
    /// Instantly fill awareness to max (instant detection).
    /// </summary>
    [ContextMenu("Instant Detect")]
    public void InstantDetect()
    {
        awareness = 1f;
    }

    /// <summary>
    /// Reset awareness to zero.
    /// </summary>
    [ContextMenu("Reset Awareness")]
    public void ResetAwareness()
    {
        awareness = 0f;
        currentState = AwarenessState.Unaware;
        timeSinceLostSight = 0f;
    }

    /// <summary>
    /// Add a fixed amount to awareness (e.g., heard a noise).
    /// </summary>
    public void AddAwareness(float amount)
    {
        awareness = Mathf.Clamp01(awareness + amount);
    }

    // ===== Gizmos =====
    private void OnDrawGizmosSelected()
    {
        // Draw awareness meter above NPC
        Vector3 meterPosition = transform.position + Vector3.up * 2.5f;

        float meterWidth = 1f;
        float meterHeight = 0.15f;

        // Background (dark)
        Gizmos.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        Gizmos.DrawCube(meterPosition, new Vector3(meterWidth, meterHeight, 0.05f));

        // Fill (colored by state)
        if (awareness > 0)
        {
            Color fillColor = GetStateColor();
            Gizmos.color = fillColor;

            float fillWidth = meterWidth * awareness;
            Vector3 fillPosition = meterPosition - Vector3.right * (meterWidth - fillWidth) * 0.5f;
            Gizmos.DrawCube(fillPosition, new Vector3(fillWidth, meterHeight * 0.8f, 0.06f));
        }

        // Threshold markers
        Gizmos.color = Color.yellow;
        float suspiciousX = meterPosition.x - meterWidth * 0.5f + meterWidth * suspiciousThreshold;
        Gizmos.DrawLine(
            new Vector3(suspiciousX, meterPosition.y - meterHeight, meterPosition.z),
            new Vector3(suspiciousX, meterPosition.y + meterHeight, meterPosition.z)
        );

        // State indicator icon
        DrawStateIcon(meterPosition + Vector3.up * 0.3f);
    }

    private void DrawStateIcon(Vector3 position)
    {
        switch (currentState)
        {
            case AwarenessState.Unaware:
                // Nothing
                break;

            case AwarenessState.Suspicious:
                // Question mark (yellow)
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(position, 0.15f);
                break;

            case AwarenessState.Detected:
                // Exclamation (red)
                Gizmos.color = Color.red;
                Gizmos.DrawCube(position, new Vector3(0.1f, 0.3f, 0.1f));
                Gizmos.DrawSphere(position - Vector3.up * 0.25f, 0.05f);
                break;
        }
    }

    private Color GetStateColor()
    {
        switch (currentState)
        {
            case AwarenessState.Suspicious:
                return Color.yellow;
            case AwarenessState.Detected:
                return Color.red;
            default:
                return Color.green;
        }
    }
}

/*
 * ============================================
 * IMPLEMENTATION STEPS FOR STUDENTS
 * ============================================
 *
 * STEP 1: SETUP SIGHT DETECTION FIRST
 * - Add SightDetection to your NPC
 * - Configure it (range, FOV, etc.)
 * - Test that it detects the player
 *
 * STEP 2: ADD AWARENESS METER
 * - Add this AwarenessMeter script (same GameObject)
 * - It will auto-find the SightDetection
 *
 * STEP 3: CONFIGURE SPEEDS
 * - Fill Speed: How fast to detect (0.5 = 2 seconds to full)
 * - Drain Speed: How fast to forget (0.3 = ~3 seconds to zero)
 * - Drain Delay: Pause before forgetting (1 = 1 second grace)
 *
 * STEP 4: CONFIGURE THRESHOLDS
 * - Suspicious Threshold: When to get curious (0.3 = 30%)
 * - Detected Threshold: When fully alerted (1.0 = 100%)
 *
 * STEP 5: CONNECT EVENTS
 * - OnSuspicious → Play "Huh?" animation, slow patrol
 * - OnFullyDetected → ChaseAction.StartChase()
 * - OnAwarenessLost → PatrolAction.StartPatrol()
 *
 * STEP 6: TEST
 * - Press Play
 * - Walk into NPC's view
 * - Watch the meter fill up!
 * - Hide and watch it drain
 *
 * ============================================
 * DIFFICULTY TUNING
 * ============================================
 *
 * EASY (Casual game):
 * - Fill Speed: 0.3 (slow detection)
 * - Drain Speed: 0.5 (fast forgetting)
 * - Drain Delay: 0 (instant forgetting)
 *
 * MEDIUM (Standard stealth):
 * - Fill Speed: 0.5
 * - Drain Speed: 0.3
 * - Drain Delay: 1
 *
 * HARD (Intense stealth):
 * - Fill Speed: 1.0 (fast detection)
 * - Drain Speed: 0.1 (slow forgetting)
 * - Drain Delay: 3 (long memory)
 *
 * ============================================
 * EXAMPLE: STEALTH GAME GUARD
 * ============================================
 *
 * Components on Guard:
 * - NavMeshAgent
 * - PatrolAction (waypoints set up)
 * - SightDetection (range: 15, FOV: 120)
 * - AwarenessMeter (this script)
 * - ChaseAction (target: Player)
 * - InvestigateAction
 *
 * Event Wiring:
 *
 * SightDetection:
 *   (Don't connect OnDetected - let AwarenessMeter handle it)
 *
 * AwarenessMeter:
 *   OnSuspicious →
 *     - PatrolAction.StopPatrol()
 *     - Play "Huh?" sound
 *
 *   OnFullyDetected →
 *     - ChaseAction.StartChase()
 *     - Play "Alert!" sound
 *
 *   OnAwarenessLost →
 *     - ChaseAction.StopChase()
 *     - InvestigateAction.Investigate(lastKnownPosition)
 *
 * InvestigateAction:
 *   OnReturnedHome →
 *     - PatrolAction.StartPatrol()
 *
 * ============================================
 * BONUS: ADD AWARENESS FROM SOUNDS
 * ============================================
 *
 * Call AddAwareness(0.2f) when player makes noise!
 *
 * Example:
 * - Player shoots gun → AddAwareness(0.5f)
 * - Player runs → AddAwareness(0.1f)
 * - Player walks → AddAwareness(0.02f)
 *
 * This adds up even if NPC can't see the player!
 *
 * ============================================
 */
