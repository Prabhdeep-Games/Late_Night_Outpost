using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

/// <summary>
/// NAVMESH ANIMATOR
/// ====================
/// Automatically syncs NPC animations with NavMeshAgent movement.
///
/// WHAT IT DOES:
/// - Watches the NavMeshAgent's velocity
/// - Sets Animator parameters automatically (Speed, IsMoving)
/// - Fires events when NPC starts/stops moving
/// - Works with ANY movement action!
///
/// REQUIREMENTS:
/// - NavMeshAgent component
/// - Animator component with proper parameters
///
/// HOW TO USE:
/// 1. Add this script to your NPC
/// 2. Set up Animator with "Speed" (float) or "IsMoving" (bool) parameter
/// 3. That's it! Animation syncs automatically
/// 4. Optionally connect events for sounds/effects
///
/// TIPS:
/// - Use a Blend Tree with "Speed" parameter for smooth walk/run blending
/// - Or use "IsMoving" bool for simple idle/walk switch
/// - Events fire only on state CHANGE (not every frame)
/// </summary>
public class NavMeshAnimator : MonoBehaviour
{
    // ===== Components =====
    [Header("Components")]
    [Tooltip("The NavMeshAgent to watch. If empty, finds one on this GameObject.")]
    public NavMeshAgent agent;

    [Tooltip("The Animator to control. If empty, finds one on this GameObject or children.")]
    public Animator animator;

    // ===== Animator Parameters =====
    [Header("Animator Parameters")]
    [Tooltip("Float parameter name for movement speed (0 = idle, 1+ = moving).")]
    public string speedParameter = "Speed";

    [Tooltip("Bool parameter name for is moving (true/false).")]
    public string isMovingParameter = "IsMoving";

    [Tooltip("Float parameter for horizontal velocity (for strafing animations).")]
    public string velocityXParameter = "";

    [Tooltip("Float parameter for forward velocity.")]
    public string velocityZParameter = "";

    // ===== Settings =====
    [Header("Settings")]
    [Tooltip("Minimum speed to be considered 'moving'.")]
    public float movingThreshold = 0.1f;

    [Tooltip("How quickly animation speed responds to velocity changes.")]
    public float smoothing = 10f;

    [Tooltip("Multiply the speed value sent to animator (for tuning blend trees).")]
    public float speedMultiplier = 1f;

    // ===== Events =====
    [Header("Events")]
    [Tooltip("Fired when NPC starts moving (was stopped, now moving).")]
    public UnityEvent OnStartedMoving;

    [Tooltip("Fired when NPC stops moving (was moving, now stopped).")]
    public UnityEvent OnStoppedMoving;

    [Tooltip("Fired every frame with current speed. Useful for UI or effects.")]
    public UnityEvent<float> OnSpeedChanged;

    // ===== State =====
    [Header("Debug (Read Only)")]
    [SerializeField] private float currentSpeed = 0f;
    [SerializeField] private bool isMoving = false;

    private float smoothedSpeed = 0f;
    private bool wasMoving = false;

    // ===== Public Properties =====
    /// <summary>
    /// Current movement speed.
    /// </summary>
    public float CurrentSpeed => currentSpeed;

    /// <summary>
    /// Is the NPC currently moving?
    /// </summary>
    public bool IsMoving => isMoving;

    // ===== Initialization =====
    private void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    // ===== Update =====
    private void Update()
    {
        if (agent == null)
            return;

        // Get current velocity
        Vector3 velocity = agent.velocity;
        currentSpeed = velocity.magnitude;

        // Smooth the speed for nicer animation transitions
        smoothedSpeed = Mathf.Lerp(smoothedSpeed, currentSpeed, Time.deltaTime * smoothing);

        // Determine if moving
        isMoving = currentSpeed > movingThreshold;

        // Update animator parameters
        UpdateAnimator(velocity);

        // Fire events on state change
        CheckStateChange();

        // Always fire speed changed event
        OnSpeedChanged?.Invoke(smoothedSpeed);
    }

    private void UpdateAnimator(Vector3 velocity)
    {
        if (animator == null)
            return;

        // Set speed parameter (float)
        if (!string.IsNullOrEmpty(speedParameter))
        {
            animator.SetFloat(speedParameter, smoothedSpeed * speedMultiplier);
        }

        // Set isMoving parameter (bool)
        if (!string.IsNullOrEmpty(isMovingParameter))
        {
            animator.SetBool(isMovingParameter, isMoving);
        }

        // Set velocity X/Z for strafing (optional)
        if (!string.IsNullOrEmpty(velocityXParameter) || !string.IsNullOrEmpty(velocityZParameter))
        {
            // Convert to local space for strafing
            Vector3 localVelocity = transform.InverseTransformDirection(velocity);

            if (!string.IsNullOrEmpty(velocityXParameter))
                animator.SetFloat(velocityXParameter, localVelocity.x);

            if (!string.IsNullOrEmpty(velocityZParameter))
                animator.SetFloat(velocityZParameter, localVelocity.z);
        }
    }

    private void CheckStateChange()
    {
        // Started moving
        if (isMoving && !wasMoving)
        {
            OnStartedMoving?.Invoke();
        }
        // Stopped moving
        else if (!isMoving && wasMoving)
        {
            OnStoppedMoving?.Invoke();
        }

        wasMoving = isMoving;
    }

    // ===== Public Methods =====

    /// <summary>
    /// Manually trigger a parameter (useful for special animations).
    /// </summary>
    public void SetTrigger(string triggerName)
    {
        if (animator != null)
            animator.SetTrigger(triggerName);
    }

    /// <summary>
    /// Manually set a bool parameter.
    /// </summary>
    public void SetBool(string paramName, bool value)
    {
        if (animator != null)
            animator.SetBool(paramName, value);
    }

    /// <summary>
    /// Play a specific animation state.
    /// </summary>
    public void PlayState(string stateName)
    {
        if (animator != null)
            animator.Play(stateName);
    }

    // ===== Gizmos =====
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
            return;

        // Show velocity vector
        if (agent != null && isMoving)
        {
            Gizmos.color = Color.cyan;
            Vector3 start = transform.position + Vector3.up * 0.1f;
            Gizmos.DrawLine(start, start + agent.velocity);
            Gizmos.DrawSphere(start + agent.velocity, 0.1f);
        }

        // Show speed as text-like indicator
        Vector3 textPos = transform.position + Vector3.up * 2f;
        Gizmos.color = isMoving ? Color.green : Color.gray;
        Gizmos.DrawWireCube(textPos, new Vector3(smoothedSpeed * 0.5f, 0.1f, 0.1f));
    }
}

/*
 * ============================================
 * IMPLEMENTATION STEPS FOR STUDENTS
 * ============================================
 *
 * STEP 1: SETUP YOUR ANIMATOR
 * - Create an Animator Controller
 * - Add parameters:
 *   - "Speed" (Float) - for blend trees
 *   - "IsMoving" (Bool) - for simple transitions
 *
 * STEP 2: CREATE ANIMATION STATES
 *
 * Option A: Simple (Bool transition)
 *   [Idle] ←──IsMoving───→ [Walk]
 *
 * Option B: Blend Tree (Float blending)
 *   [Locomotion Blend Tree]
 *   ├── Speed 0.0 → Idle
 *   ├── Speed 0.5 → Walk
 *   └── Speed 1.0 → Run
 *
 * STEP 3: ADD NAVMESH ANIMATOR
 * - Add this script to your NPC
 * - It will auto-find NavMeshAgent and Animator
 * - Or drag them in manually
 *
 * STEP 4: CONFIGURE PARAMETERS
 * - Speed Parameter: "Speed" (must match Animator!)
 * - Is Moving Parameter: "IsMoving" (must match!)
 * - Leave blank if not using that parameter
 *
 * STEP 5: CONNECT EVENTS (OPTIONAL)
 * - OnStartedMoving → Play footstep sound
 * - OnStoppedMoving → Play arrive sound
 *
 * STEP 6: TEST
 * - Press Play
 * - Use any movement action (Patrol, Chase, etc.)
 * - Animation plays automatically!
 *
 * ============================================
 * ANIMATOR SETUP GUIDE
 * ============================================
 *
 * SIMPLE SETUP (Idle/Walk toggle):
 *
 * 1. Create Animator Controller
 * 2. Add "IsMoving" Bool parameter
 * 3. Create states: Idle, Walk
 * 4. Add transitions:
 *    - Idle → Walk (Condition: IsMoving = true)
 *    - Walk → Idle (Condition: IsMoving = false)
 * 5. Uncheck "Has Exit Time" on both transitions!
 *
 * ============================================
 * BLEND TREE SETUP (Smooth blending):
 *
 * 1. Create Animator Controller
 * 2. Add "Speed" Float parameter
 * 3. Create Blend Tree state
 * 4. Open Blend Tree, set parameter to "Speed"
 * 5. Add motions:
 *    - Threshold 0: Idle animation
 *    - Threshold 3: Walk animation
 *    - Threshold 6: Run animation
 * 6. Adjust "Speed Multiplier" in NavMeshAnimator
 *    to match your NavMeshAgent speed
 *
 * ============================================
 * EVENT EXAMPLES
 * ============================================
 *
 * OnStartedMoving:
 * - AudioSource.Play() → footstep loop
 * - ParticleSystem.Play() → dust particles
 *
 * OnStoppedMoving:
 * - AudioSource.Stop() → stop footsteps
 * - ParticleSystem.Stop() → stop dust
 * - Play "Arrive" sound effect
 *
 * OnSpeedChanged(float):
 * - Adjust footstep audio pitch based on speed
 * - Scale dust particle emission by speed
 *
 * ============================================
 * TROUBLESHOOTING
 * ============================================
 *
 * Animation not playing?
 * - Check parameter names match EXACTLY
 * - Check Animator Controller is assigned
 * - Make sure transitions don't have "Has Exit Time"
 *
 * Animation stuttering?
 * - Increase "Smoothing" value
 * - Check NavMeshAgent speed isn't fluctuating
 *
 * Walk animation too slow/fast?
 * - Adjust "Speed Multiplier"
 * - Or adjust Blend Tree thresholds
 *
 * ============================================
 */
