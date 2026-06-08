// ============================================================================
// PlayerAttack — Glue / input layer
//
// One per player. Owns the Attack input subscription. Each frame on key-down,
// it asks the GrabAnchor "what am I holding?" and, if that prop has a Weapon
// component, calls Fire on it. The Weapon delegates to its Spawner, the
// Spawner instantiates the attack prefab — game logic lives in the prefab.
//
// Three composed references, no inheritance, no inventory, no equip state:
//   GrabAnchor          — exposes the currently held Grabbable
//   InputActionReference — the Attack action in your InputActions asset
//   Weapon (per prop)   — the role marker that names a Spawner as the fire mechanism
//
// "What's equipped" = "what's in my hand." Drop the prop → no held → no fire.
// Pick up a different prop → its Weapon's Spawner fires instead. Unlock = the
// world letting the player reach that prop.
// ============================================================================

using UnityEngine;
using UnityEngine.InputSystem;

namespace Ludocore
{
    /// <summary>Player-side trigger for wielded attacks. On Attack input, fires
    /// the Weapon on whatever Grabbable the GrabAnchor currently holds.</summary>
    public class PlayerAttack : MonoBehaviour
    {
        //==================== CONFIG =====================
        [Header("Config")]
        [Tooltip("Grab anchor that tracks the currently held prop. Usually the " +
                 "child of the camera that Grabbables follow.")]
        [SerializeField] private GrabAnchor anchor;

        [Tooltip("Input action that fires the held weapon — define in your " +
                 "InputActions asset (Player map → Attack, bound to Mouse0 / Gamepad West).")]
        [SerializeField] private InputActionReference attackAction;

        //==================== LIFECYCLE =====================
        private void OnEnable()
        {
            if (attackAction) attackAction.action.Enable();
        }

        private void OnDisable()
        {
            if (attackAction) attackAction.action.Disable();
        }

        private void Update()
        {
            if (!attackAction) return;
            if (!attackAction.action.WasPressedThisFrame()) return;

            if (!anchor) return;
            if (!anchor.Held) return;

            if (anchor.Held.TryGetComponent(out Weapon weapon)) weapon.Fire();
        }
    }
}

// ============================================================================
// Setup in a scene
//   1. On the player root (or camera): add this PlayerAttack component.
//   2. Wire `anchor` to the scene's active GrabAnchor (the child of the camera).
//   3. In your InputActions asset (e.g. InputSystem_Actions.inputactions):
//      - Add an Attack action under the Player map (Button type).
//      - Bind it to Mouse / Left Button and Gamepad / West (or your choice).
//   4. Drag the Attack InputActionReference into `attackAction` on PlayerAttack.
//   5. On each wieldable prop in the scene: add a Spawner (LocalSpawner) +
//      Weapon (pointed at that Spawner). See Weapon.cs for prop-side setup.
//   6. Press play. Pick up a weapon prop with E, press Attack to fire.
//      Drop the prop → Attack does nothing. Pick up a different weapon →
//      that one fires instead. Zero extra wiring per weapon variant.
//
// Note on input systems
//   This component uses the new Input System (InputActionReference). The
//   project's Active Input Handling (Project Settings → Player) must be
//   "Input System Package (New)" or "Both" for this to fire. PlayerInteractor
//   follows the same pattern.
// ============================================================================
