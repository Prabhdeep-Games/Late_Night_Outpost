// ============================================================================
// PlayerBuild — Glue / input layer (mirror of PlayerAttack)
//
// One per player. Owns the Place input subscription and a raycast for the
// placement target. Each frame:
//   1. Ask the GrabAnchor "what am I holding?"
//   2. If it has a Builder, drive a ghost preview at the raycast hit point
//   3. On place-input key-down, call Builder.TryPlace(hit, rotation)
//
// The ghost preview is regenerated whenever the held Builder changes (pick
// up a different tool → different ghost). Destroyed when the player drops
// the Builder or this component disables.
//
// Three composed references, no inheritance, no inventory, no equip state:
//   GrabAnchor          — exposes the currently held Grabbable
//   RaycastSensor       — forward raycast that finds the target surface
//   InputActionReference — the Place action in your InputActions asset
//
// "What's equipped" = "what's in my hand." Same lesson as PlayerAttack.
// Drop the builder prop → ghost disappears, input does nothing. Pick up
// a different builder → its ghost appears, its prefab is what gets placed.
// ============================================================================

using UnityEngine;
using UnityEngine.InputSystem;

namespace Ludocore
{
    /// <summary>Player-side trigger for held-builder placement. Drives the ghost
    /// preview each frame; on place input fires the held Builder's TryPlace.</summary>
    public class PlayerBuild : MonoBehaviour
    {
        //==================== CONFIG =====================
        [Header("Config")]
        [Tooltip("Grab anchor that tracks the currently held prop.")]
        [SerializeField] private GrabAnchor anchor;

        [Tooltip("Forward raycast that finds the placement surface. Typically the same " +
                 "sensor PlayerInteractor uses. Its layer mask should include the ground.")]
        [SerializeField] private RaycastSensor raycastSensor;

        [Tooltip("Input action that confirms placement. Often the same Attack action — " +
                 "the held prop disambiguates which verb runs.")]
        [SerializeField] private InputActionReference placeAction;

        [Header("Ghost")]
        [Tooltip("Material applied to the ghost preview while a Builder is held.")]
        [SerializeField] private Material ghostMaterial;

        [Tooltip("If true, the ghost rotates to face this transform's forward direction " +
                 "(yaw only, pitch ignored). If false, the ghost spawns at world rotation.")]
        [SerializeField] private bool alignGhostToPlayer = true;

        //==================== STATE =====================
        [Header("Debug")]
        [ReadOnly, SerializeField] private Builder currentBuilder;

        private GameObject _ghost;
        private BuildingData _ghostFor;

        // Cache the Builder resolution by held identity — TryGetComponent is too
        // expensive to call every frame, but the held prop changes rarely.
        private Grabbable _cachedHeld;
        private Builder _cachedBuilder;

        //==================== LIFECYCLE =====================
        private void OnEnable()
        {
            if (placeAction) placeAction.action.Enable();
        }

        private void OnDisable()
        {
            if (placeAction) placeAction.action.Disable();
            ClearGhost();
            currentBuilder = null;
            _cachedHeld = null;
            _cachedBuilder = null;
        }

        private void Update()
        {
            currentBuilder = ResolveHeldBuilder();

            // Builder changed → rebuild the ghost to match the new prefab.
            BuildingData wanted = currentBuilder ? currentBuilder.Data : null;
            if (wanted != _ghostFor) RebuildGhost(wanted);

            if (!currentBuilder || !raycastSensor || !raycastSensor.IsHitting)
            {
                SetGhostActive(false);
                return;
            }

            Vector3 cursor = raycastSensor.HitPoint;
            Quaternion rot = GetPlacementRotation();

            // Drive the ghost at the FINAL resolved spawn position (offset + snap),
            // so the preview matches exactly where TryPlace will place the building.
            Vector3 ghostPos = currentBuilder.ResolveSpawnPosition(cursor, rot);
            PositionGhost(ghostPos, rot);
            SetGhostActive(true);

            if (placeAction && placeAction.action.WasPressedThisFrame())
                currentBuilder.TryPlace(cursor, rot);
        }

        //==================== PRIVATE =====================
        private Builder ResolveHeldBuilder()
        {
            Grabbable held = anchor ? anchor.Held : null;

            // Same prop as last frame → reuse the cached lookup (avoids per-frame TryGetComponent).
            if (held == _cachedHeld) return _cachedBuilder;

            _cachedHeld = held;
            _cachedBuilder = null;
            if (held) held.TryGetComponent(out _cachedBuilder);
            return _cachedBuilder;
        }

        private Quaternion GetPlacementRotation()
        {
            if (!alignGhostToPlayer) return Quaternion.identity;

            Vector3 forward = transform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.0001f) return Quaternion.identity;
            return Quaternion.LookRotation(forward, Vector3.up);
        }

        private void RebuildGhost(BuildingData data)
        {
            ClearGhost();
            _ghostFor = data;
            if (data == null || !data.Prefab) return;
            _ghost = GhostPreview.Create(data.Prefab, transform.position, Quaternion.identity, ghostMaterial);
        }

        private void ClearGhost()
        {
            if (_ghost) Destroy(_ghost);
            _ghost = null;
            _ghostFor = null;
        }

        private void SetGhostActive(bool on)
        {
            if (_ghost && _ghost.activeSelf != on) _ghost.SetActive(on);
        }

        private void PositionGhost(Vector3 p, Quaternion r)
        {
            if (_ghost) _ghost.transform.SetPositionAndRotation(p, r);
        }
    }
}

// ============================================================================
// Setup in a scene
//   1. On the player root (or camera): add this PlayerBuild component.
//   2. Wire:
//      - anchor — the scene's active GrabAnchor (same one PlayerAttack uses)
//      - raycastSensor — the camera's forward RaycastSensor (often the same
//        one PlayerInteractor uses; make sure its layer mask includes terrain
//        / ground surfaces where buildings should sit)
//      - placeAction — an InputActionReference (Mouse Left / Gamepad West).
//        Often the same action as PlayerAttack's, since the held prop
//        disambiguates which verb runs.
//      - ghostMaterial — the blue transparent material
//   3. On each builder prop: add a Grabbable + a Builder pointing at a
//      BuildingData asset (with its Prefab field set).
//   4. Press play. Pick up a builder prop with E, look around — a ghost
//      follows the cursor. Click to place. Drop the prop → ghost disappears,
//      click does nothing.
//
// Note on input systems
//   This component uses the new Input System (InputActionReference). The
//   project's Active Input Handling (Project Settings → Player) must be
//   "Input System Package (New)" or "Both" for this to fire.
// ============================================================================
