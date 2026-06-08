// ============================================================================
// BuildingSite — Logic layer
//
// A location where a building can be materialized — and, if the data defines
// an upgrade chain, progressively upgraded in place. Call Build() and it
// consumes the cost, spawns nextData.Prefab (replacing the previous instance
// if any), advances along the chain, and fires OnBuilt(previous, current).
//
// State model
//   currentData     — what is currently standing (null = empty plot)
//   nextData        — what Build() will produce next (null = fully maxed out)
//   _currentInstance — the spawned GameObject for the current level
//
//   Flow: nextData = startingData. Build() consumes nextData.Costs, destroys
//   _currentInstance (if any), spawns a fresh instance of nextData.Prefab at
//   this site's pose, then currentData ← nextData, nextData ← currentData.NextLevel.
//   Once nextData becomes null the site is maxed — CanBuild is naturally false.
//
// The site itself persists across upgrades — it stays put, focused, with a
// prompt for the next tier. The Building instance is what gets swapped.
//
// Implements IBuildable (verb data + instant action) AND IInteractable
// (tap dispatch — single key-down completes Build() immediately).
// PlayerInteractor reads only IInteractable; UI/listeners read IBuildable.
// Focus/detection is handled by a sibling Focusable.
// ============================================================================

using System;
using UnityEngine;
using UnityEngine.Events;

namespace Ludocore
{
    /// <summary>Logic for a build site — guards, consumes cost, advances along an upgrade chain.</summary>
    public class BuildingSite : MonoBehaviour, IBuildable, IInteractable
    {
        //==================== CONFIG =====================
        [Header("Config")]
        [Tooltip("The first level the site starts at. Subsequent levels come from BuildingData.NextLevel.")]
        [SerializeField] private BuildingData startingData;

        [Header("Placement")]
        [Tooltip("Local-space offset applied to the spawn position (rotated by this site's rotation). " +
                 "Push the building off the site marker so the marker stays visible. " +
                 "Z is forward, X is right, Y is up.")]
        [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 0f, 1f);

        [Tooltip("If true, the final spawn position is snapped down to the nearest collider on the ground mask. " +
                 "Recommended for terrain — keeps the building flush even when the site marker is floating.")]
        [SerializeField] private bool snapToGround = true;

        [Tooltip("Which layers count as ground for the snap raycast.")]
        [SerializeField] private LayerMask groundMask = ~0;

        [Tooltip("Vertical search range for the ground snap. Cast starts this far above the offset position " +
                 "and reaches twice this distance downward.")]
        [Min(0.01f)]
        [SerializeField] private float snapHeight = 5f;

        //==================== STATE =====================
        [Header("Debug")]
        [Tooltip("What is currently standing on the site (null = empty plot).")]
        [ReadOnly, SerializeField] private BuildingData currentData;

        [Tooltip("What Build() will produce next (null = fully upgraded).")]
        [ReadOnly, SerializeField] private BuildingData nextData;

        private GameObject _currentInstance;

        // IBuildable — Data exposes the NEXT thing to build (UI reads this to show the prompt).
        public BuildingData Data => nextData;
        public bool CanBuild => nextData && nextData.Prefab && CanAfford;

        /// <summary>What is currently standing on this plot (null = empty).</summary>
        public BuildingData CurrentData => currentData;

        /// <summary>The spawned GameObject for the current level (null = nothing built yet).</summary>
        public GameObject CurrentInstance => _currentInstance;

        /// <summary>Final world spawn position — site transform + offset (rotated) + optional ground snap.
        /// Use this to position previews so they match where Build() will actually spawn.</summary>
        public Vector3 SpawnPosition
        {
            get
            {
                Vector3 world = transform.position + transform.rotation * spawnOffset;
                return snapToGround ? SnapDownToGround(world) : world;
            }
        }

        /// <summary>Is anything standing on this plot yet?</summary>
        public bool HasBuilding => currentData;

        /// <summary>True when no further upgrade exists (nothing left to build).</summary>
        public bool IsMaxed => !nextData;

        // IInteractable — tap verb.
        public bool CanInteract => CanBuild;

        /// <summary>True when the next level is free or the player has enough of every cost in its list.</summary>
        public bool CanAfford
        {
            get
            {
                if (!nextData || nextData.IsFree) return true;
                if (!ResourceManager.Instance) return false;
                return ResourceManager.Instance.CanAfford(nextData.Costs);
            }
        }

        //==================== OUTPUTS =====================
        /// <summary>Fired after a successful build. Args: (previous data — null on first build, new currentData).</summary>
        public event Action<BuildingData, BuildingData> OnBuilt;

        [Header("Events")]
        [Tooltip("Fired after every successful build/upgrade — wire VFX / SFX here.")]
        [SerializeField] private UnityEvent builtEvent;

        //==================== LIFECYCLE =====================
        private void Awake()
        {
            nextData = startingData;
        }

        //==================== INPUTS =====================
        /// <summary>IInteractable entry point. Tap verb — completes Build() immediately.</summary>
        public void BeginInteract() => Build();

        /// <summary>Consume the cost (if any), spawn the next tier's prefab in place
        /// (replacing the previous instance), and advance along the upgrade chain.</summary>
        [ContextMenu("Build")]
        public void Build()
        {
            if (!CanBuild) return;

            // Consume resources through TryConsume — authoritative gate. Free levels skip.
            if (!nextData.IsFree)
            {
                if (!ResourceManager.Instance) return;
                if (!ResourceManager.Instance.TryConsume(nextData.Costs)) return;
            }

            // Swap the standing building for the new tier — destroy the old, spawn the new
            // at the resolved spawn position (site + offset + ground snap).
            if (_currentInstance) Destroy(_currentInstance);
            _currentInstance = Instantiate(nextData.Prefab, SpawnPosition, transform.rotation);

            BuildingData previous = currentData;
            currentData = nextData;
            nextData = currentData.NextLevel;

            OnBuilt?.Invoke(previous, currentData);
            builtEvent?.Invoke();
        }

        //==================== PRIVATE =====================
        private Vector3 SnapDownToGround(Vector3 pos)
        {
            Vector3 origin = pos + Vector3.up * snapHeight;
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, snapHeight * 2f, groundMask))
                return hit.point;
            return pos;
        }

        private void OnDrawGizmosSelected()
        {
            // Visualize the offset + final spawn point so designers can tune placement without entering play mode.
            Vector3 offsetWorld = transform.position + transform.rotation * spawnOffset;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, offsetWorld);
            Gizmos.DrawWireSphere(offsetWorld, 0.15f);

            if (snapToGround && Application.isPlaying)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(SpawnPosition, 0.2f);
            }
        }
    }
}

// ============================================================================
// Setup in a scene
//   1. Create an empty GameObject at the desired build location.
//   2. Add a small Collider (not a trigger — the raycast needs to hit it).
//      Keep it small / low so it doesn't obscure the ghost preview behind it.
//   3. Add this BuildingSite component and assign a BuildingData asset as
//      Starting Data. The data's Prefab field must be set. To make the
//      building upgradable, set NextLevel on the data (and so on along the chain).
//   4. Add a Focusable component (required by BuildingSiteView / PlayerInteractor).
//   5. (Optional) Add BuildingSiteView for ghost preview, particles, prompt UI.
//   6. (Optional) Hook particles / SFX / quest hooks to the builtEvent UnityEvent.
//   7. Make sure the player has a PlayerInteractor + RaycastSensor, and the
//      site's collider layer is included in the sensor's layerMask.
//
// On build the site replaces the standing instance (if any) with the new
// tier's prefab at the same pose. The site itself persists across all
// upgrades — it only "deactivates" by virtue of CanBuild becoming false
// when the chain is exhausted (IsMaxed). The prefab's own intro animation
// (e.g. BuildingEmerge) plays each time a tier is spawned.
// ============================================================================
