using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// SIGHT DETECTION
/// ====================
/// Allows an NPC to detect targets using vision.
///
/// WHAT IT DOES:
/// - Checks if a target is within range
/// - Checks if NPC can actually SEE the target (line of sight)
/// - Checks if target is within field of view angle
/// - Fires events when target is detected or lost
///
/// REQUIREMENTS:
/// - Target must have a collider (for raycast)
/// - Target should have the correct tag
///
/// HOW TO USE:
/// 1. Add this script to your NPC
/// 2. Set the "Target Tag" (e.g., "Player")
/// 3. Adjust range and field of view
/// 4. Connect OnDetected/OnLost to other actions
///
/// TIPS:
/// - Set FOV to 360 for all-around vision (like a turret)
/// - Set FOV to 90-120 for realistic human vision
/// - Use with AwarenessMeter for stealth games
/// </summary>
public class SightDetection : MonoBehaviour
{
    // ===== Target Settings =====
    [Header("Target")]
    [Tooltip("Tag of objects to detect (e.g., 'Player').")]
    public string targetTag = "Player";

    [Tooltip("Current detected target (auto-filled, or set manually).")]
    public Transform currentTarget;

    // ===== Detection Settings =====
    [Header("Detection")]
    [Tooltip("Maximum distance the NPC can see.")]
    public float detectionRange = 15f;

    [Tooltip("Field of view angle in degrees. 360 = all around, 90 = narrow cone.")]
    [Range(10f, 360f)]
    public float fieldOfView = 120f;

    [Tooltip("Height offset for the raycast origin (eye level).")]
    public float eyeHeight = 1.5f;

    [Tooltip("Layers that block line of sight (walls, obstacles).")]
    public LayerMask obstacleLayers = ~0; // Default: everything

    // ===== Check Settings =====
    [Header("Check Settings")]
    [Tooltip("How often to check for targets (in seconds). Lower = more responsive but costly.")]
    public float checkInterval = 0.2f;

    // ===== Events =====
    [Header("Events")]
    [Tooltip("Fired when a target is first detected.")]
    public UnityEvent OnDetected;

    [Tooltip("Fired when the target is lost (out of range, out of sight, etc.).")]
    public UnityEvent OnLost;

    // ===== State =====
    [Header("Debug (Read Only)")]
    [SerializeField] private bool canSeeTarget = false;
    [SerializeField] private float distanceToTarget = 0f;
    [SerializeField] private float angleToTarget = 0f;

    private float nextCheckTime = 0f;
    private bool wasDetected = false;

    // ===== Public Properties =====
    /// <summary>
    /// Returns true if the NPC can currently see the target.
    /// </summary>
    public bool CanSeeTarget => canSeeTarget;

    /// <summary>
    /// Returns the last known position of the target.
    /// </summary>
    public Vector3 LastKnownPosition { get; private set; }

    // ===== Update =====
    private void Update()
    {
        // Check at intervals (not every frame)
        if (Time.time < nextCheckTime)
            return;

        nextCheckTime = Time.time + checkInterval;

        // Find target if not set
        if (currentTarget == null)
        {
            FindTarget();
        }

        // Check visibility
        bool canSeeNow = CheckVisibility();

        // Fire events on state change
        if (canSeeNow && !wasDetected)
        {
            // Just detected!
            wasDetected = true;
            OnDetected?.Invoke();
        }
        else if (!canSeeNow && wasDetected)
        {
            // Just lost!
            wasDetected = false;
            OnLost?.Invoke();
        }

        canSeeTarget = canSeeNow;
    }

    // ===== Detection Logic =====
    private void FindTarget()
    {
        GameObject targetObject = GameObject.FindGameObjectWithTag(targetTag);
        if (targetObject != null)
        {
            currentTarget = targetObject.transform;
        }
    }

    private bool CheckVisibility()
    {
        if (currentTarget == null)
            return false;

        Vector3 eyePosition = transform.position + Vector3.up * eyeHeight;
        Vector3 targetPosition = currentTarget.position + Vector3.up * eyeHeight * 0.5f;
        Vector3 directionToTarget = targetPosition - eyePosition;

        // === CHECK 1: Distance ===
        distanceToTarget = directionToTarget.magnitude;
        if (distanceToTarget > detectionRange)
            return false;

        // === CHECK 2: Field of View ===
        Vector3 flatDirection = new Vector3(directionToTarget.x, 0, directionToTarget.z).normalized;
        Vector3 flatForward = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;
        angleToTarget = Vector3.Angle(flatForward, flatDirection);

        if (angleToTarget > fieldOfView * 0.5f)
            return false;

        // === CHECK 3: Line of Sight (Raycast) ===
        if (Physics.Raycast(eyePosition, directionToTarget.normalized, out RaycastHit hit, detectionRange, obstacleLayers))
        {
            // Did we hit the target or something else?
            if (hit.transform != currentTarget && !hit.transform.IsChildOf(currentTarget))
            {
                // Hit a wall or obstacle
                return false;
            }
        }

        // All checks passed - we can see the target!
        LastKnownPosition = currentTarget.position;
        return true;
    }

    // ===== Public Methods =====

    /// <summary>
    /// Manually set a new target to track.
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        currentTarget = newTarget;
    }

    /// <summary>
    /// Clear the current target.
    /// </summary>
    public void ClearTarget()
    {
        currentTarget = null;
        canSeeTarget = false;
        wasDetected = false;
    }

    /// <summary>
    /// Force an immediate visibility check.
    /// </summary>
    [ContextMenu("Check Now")]
    public void CheckNow()
    {
        nextCheckTime = 0f;
    }

    // ===== Gizmos =====
    private void OnDrawGizmosSelected()
    {
        Vector3 eyePosition = transform.position + Vector3.up * eyeHeight;

        // Detection range sphere
        Gizmos.color = new Color(1f, 1f, 0f, 0.1f);
        Gizmos.DrawSphere(transform.position, detectionRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Field of view cone
        DrawFOVCone(eyePosition);

        // Line of sight to target
        if (currentTarget != null)
        {
            Vector3 targetPosition = currentTarget.position + Vector3.up * eyeHeight * 0.5f;

            if (canSeeTarget)
            {
                // Can see - green line
                Gizmos.color = Color.green;
                Gizmos.DrawLine(eyePosition, targetPosition);
                Gizmos.DrawWireSphere(targetPosition, 0.3f);
            }
            else
            {
                // Cannot see - red line
                Gizmos.color = Color.red;
                Gizmos.DrawLine(eyePosition, targetPosition);
            }
        }

        // Eye position indicator
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(eyePosition, 0.1f);
    }

    private void DrawFOVCone(Vector3 origin)
    {
        if (fieldOfView >= 360f)
            return; // No need to draw cone for 360 vision

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);

        float halfFOV = fieldOfView * 0.5f;
        Vector3 leftDir = Quaternion.Euler(0, -halfFOV, 0) * transform.forward;
        Vector3 rightDir = Quaternion.Euler(0, halfFOV, 0) * transform.forward;

        // Draw cone edges
        Gizmos.DrawRay(origin, leftDir * detectionRange);
        Gizmos.DrawRay(origin, rightDir * detectionRange);

        // Draw arc
        int segments = 20;
        Vector3 prevPoint = origin + leftDir * detectionRange;
        for (int i = 1; i <= segments; i++)
        {
            float angle = -halfFOV + (fieldOfView * i / segments);
            Vector3 dir = Quaternion.Euler(0, angle, 0) * transform.forward;
            Vector3 point = origin + dir * detectionRange;
            Gizmos.DrawLine(prevPoint, point);
            prevPoint = point;
        }
    }
}

/*
 * ============================================
 * IMPLEMENTATION STEPS FOR STUDENTS
 * ============================================
 *
 * STEP 1: SETUP THE NPC
 * - Add this SightDetection script to your NPC
 * - Make sure the NPC faces forward (blue arrow in Unity)
 *
 * STEP 2: TAG YOUR PLAYER
 * - Select the Player GameObject
 * - Set its Tag to "Player" (or your chosen tag)
 * - Make sure player has a Collider!
 *
 * STEP 3: CONFIGURE VISION
 * - Detection Range: How far can the NPC see? (e.g., 15)
 * - Field of View: How wide? (120 = human-like, 360 = turret)
 * - Eye Height: Where are the NPC's "eyes"? (e.g., 1.5)
 *
 * STEP 4: SET OBSTACLE LAYERS
 * - Create a layer for walls/obstacles
 * - Set "Obstacle Layers" to include that layer
 * - Now NPC can't see through walls!
 *
 * STEP 5: CONNECT EVENTS
 * - OnDetected → ChaseAction.StartChase()
 * - OnLost → InvestigateAction.Investigate()
 *
 * STEP 6: TEST
 * - Press Play
 * - Walk into the NPC's view
 * - Watch the gizmos change color!
 *
 * ============================================
 * GIZMO GUIDE
 * ============================================
 *
 * YELLOW SPHERE = Detection range
 * ORANGE CONE = Field of view
 * CYAN DOT = Eye position
 * GREEN LINE = Can see target!
 * RED LINE = Cannot see target (blocked or out of FOV)
 *
 * ============================================
 * EXAMPLE: BASIC ENEMY
 * ============================================
 *
 * SightDetection settings:
 * - Target Tag: "Player"
 * - Detection Range: 15
 * - Field of View: 120
 *
 * Events:
 * - OnDetected → ChaseAction.StartChase()
 * - OnLost → PatrolAction.StartPatrol()
 *
 * Result: Enemy chases when it sees you, patrols when it doesn't!
 *
 * ============================================
 */
