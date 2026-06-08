// ============================================================================
// Builder — Role marker on a held prop (held-prop placement verb)
//
// Sibling of Weapon. Sits on a grabbable prop alongside a Grabbable /
// HoldGrabbable. Names the prop's purpose: "while held, this prop places
// a building." PlayerBuild queries the held Grabbable for a Builder and
// calls TryPlace(position, rotation) when the place input fires.
//
// The Builder owns WHAT to build (the BuildingData) and the resource-cost
// gate. PlayerBuild owns WHERE (raycast target) and WHEN (input). Same
// split as Weapon (data) + PlayerAttack (input).
//
// Unlimited uses while the player can afford the cost. The prop stays in
// hand — picking it up "unlocks" the verb, dropping it removes the verb.
// "What's equipped" = "what's in my hand," same lesson as PlayerAttack.
// ============================================================================

using System;
using UnityEngine;
using UnityEngine.Events;

namespace Ludocore
{
    /// <summary>Marks a prop as a building tool. On TryPlace, consumes the cost
    /// and instantiates BuildingData.Prefab at the requested pose.</summary>
    public class Builder : MonoBehaviour
    {
        //==================== CONFIG =====================
        [Header("Config")]
        [Tooltip("Which building this tool places.")]
        [SerializeField] private BuildingData data;

        [Header("Placement")]
        [Tooltip("Local-space offset applied relative to the placement rotation. " +
                 "Push the building off the cursor — useful when the prefab's pivot isn't at its base center, " +
                 "or to nudge the building slightly ahead of the player's aim. Z is forward, X is right, Y is up.")]
        [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 0f, 1f);

        [Tooltip("If true, the final spawn position is snapped down to the nearest collider on the ground mask. " +
                 "Recommended for terrain — keeps the building flush even when the cursor is on a slope.")]
        [SerializeField] private bool snapToGround = true;

        [Tooltip("Which layers count as ground for the snap raycast.")]
        [SerializeField] private LayerMask groundMask = ~0;

        [Tooltip("Vertical search range for the ground snap. Cast starts this far above the offset position " +
                 "and reaches twice this distance downward.")]
        [Min(0.01f)]
        [SerializeField] private float snapHeight = 5f;

        public BuildingData Data => data;

        /// <summary>True when the data is valid and the player can afford the cost (or it's free).</summary>
        public bool CanPlace
        {
            get
            {
                if (!data || !data.Prefab) return false;
                if (data.IsFree) return true;
                return ResourceManager.Instance && ResourceManager.Instance.CanAfford(data.Costs);
            }
        }

        //==================== OUTPUTS =====================
        /// <summary>Fired after a successful placement — passes the spawned instance.</summary>
        public event Action<GameObject> OnPlaced;

        [Header("Events")]
        [Tooltip("Fired after a successful placement — passes the spawned instance.")]
        [SerializeField] private UnityEvent<GameObject> placedEvent;

        //==================== INPUTS =====================
        /// <summary>Compute the final spawn position for a given cursor target — applies the
        /// local offset (rotated by the placement rotation) and optional ground snap.
        /// Use this to position the ghost preview so it matches what TryPlace will actually spawn.</summary>
        public Vector3 ResolveSpawnPosition(Vector3 cursorPosition, Quaternion rotation)
        {
            Vector3 world = cursorPosition + rotation * spawnOffset;
            return snapToGround ? SnapDownToGround(world) : world;
        }

        /// <summary>Pay the cost and instantiate the building at the resolved spawn pose.
        /// `cursorPosition` is the raw aim target (e.g. raycast hit point); offset + snap
        /// are applied internally. Returns true on success.</summary>
        public bool TryPlace(Vector3 cursorPosition, Quaternion rotation)
        {
            if (!CanPlace) return false;
            if (!data.IsFree && !ResourceManager.Instance.TryConsume(data.Costs)) return false;

            Vector3 spawnPos = ResolveSpawnPosition(cursorPosition, rotation);
            GameObject instance = Instantiate(data.Prefab, spawnPos, rotation);
            OnPlaced?.Invoke(instance);
            placedEvent?.Invoke(instance);
            return true;
        }

        //==================== PRIVATE =====================
        private Vector3 SnapDownToGround(Vector3 pos)
        {
            Vector3 origin = pos + Vector3.up * snapHeight;
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, snapHeight * 2f, groundMask))
                return hit.point;
            return pos;
        }
    }
}

// ============================================================================
// Setup on a held builder prop
//   1. Add a Grabbable (or HoldGrabbable) so the player can pick the prop up.
//   2. Add this Builder component. Drag a BuildingData asset into the data field
//      (the BuildingData's Prefab field must be set).
//   3. On the player, a PlayerBuild component will call TryPlace when the
//      place input fires AND this prop is the one currently held.
//   4. The prop stays in hand — unlimited placements while the player can
//      afford the cost. Wire placedEvent for per-place effects (SFX, particles,
//      "blueprint exhausted" logic, etc.).
// ============================================================================
