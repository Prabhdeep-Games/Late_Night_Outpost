// ============================================================================
// Harvestable — Logic layer
//
// A GameObject that can be harvested for a resource.
//
// The player flow is HOLD-to-harvest, driven through IHoldInteractable:
// BeginInteract() starts a timer that ticks in Update(); CancelInteract()
// resets it; once progress hits 1.0 the harvest completes — yield is
// deposited into ResourceManager, events fire, and the GameObject is
// destroyed after a short delay so VFX/SFX have time to play.
//
// Harvest() is preserved as the instant path for NPCs (e.g. WorkerController),
// scripts, debug, and the context menu — anything that doesn't need a held
// timer. It satisfies IHarvestable.
//
// Implements IHarvestable (verb data + instant action) AND IHoldInteractable
// (player-driven held interaction). PlayerInteractor reads only IInteractable
// (the base of IHoldInteractable); NPCs read IHarvestable.
//
// This script has a single responsibility: harvest. It does NOT know about
// the player, sensors, or focus. Detection lives on PlayerInteractor; focus
// state lives on a sibling Focusable component; the radial fill UI lives on
// a sibling InteractableRadial that binds to OnProgress.
// ============================================================================

using System;
using UnityEngine;
using UnityEngine.Events;

namespace Ludocore
{
    /// <summary>Logic for a harvestable GameObject — drives a hold-to-harvest
    /// timer (IHoldInteractable) and deposits yield to ResourceManager when
    /// progress completes. Also exposes IHarvestable for NPC-driven instant
    /// harvest.</summary>
    public class Harvestable : MonoBehaviour, IHarvestable, IHoldInteractable
    {
        //==================== CONFIG =====================
        [Header("Config")]
        [Tooltip("Resource type yielded on harvest.")]
        [SerializeField] private ResourceData resource;

        [Tooltip("Amount of the resource yielded per harvest.")]
        [Min(0)]
        [SerializeField] private int yieldAmount = 1;

        [Tooltip("Seconds the player must hold to complete the harvest. 0 = instant.")]
        [Min(0f)]
        [SerializeField] private float harvestDuration = 1.5f;

        [Tooltip("Delay before the GameObject is destroyed after harvest — lets VFX/SFX finish.")]
        [Min(0f)]
        [SerializeField] private float destroyDelay = 0.5f;

        //==================== STATE =====================
        [Header("Debug")]
        [ReadOnly, SerializeField] private bool isHarvested;
        [ReadOnly, SerializeField] private bool isInteracting;
        [ReadOnly, SerializeField] private float progress;

        // IHarvestable
        public ResourceData Resource => resource;
        public int Yield => yieldAmount;
        public bool CanHarvest => !isHarvested && resource;

        // IHoldInteractable
        public float Duration => harvestDuration;
        public bool CanInteract => CanHarvest;

        // Read-only state for UI / debug.
        public float Progress => progress;
        public bool IsInteracting => isInteracting;

        //==================== OUTPUTS =====================
        public event Action OnHarvested;
        public event Action<float> OnProgress;

        [Header("Events")]
        [Tooltip("Fired when the harvest succeeds — wire VFX / SFX here.")]
        [SerializeField] private UnityEvent harvestedEvent;

        //==================== INPUTS =====================
        /// <summary>IHoldInteractable — begin the held-harvest timer. Idempotent while in progress.</summary>
        public void BeginInteract()
        {
            if (!CanInteract || isInteracting) return;
            isInteracting = true;
        }

        /// <summary>IHoldInteractable — cancel the held-harvest timer and reset progress to zero.</summary>
        public void CancelInteract()
        {
            if (!isInteracting) return;
            isInteracting = false;
            progress = 0f;
            OnProgress?.Invoke(0f);
        }

        /// <summary>IHarvestable — instant harvest, bypasses the held timer. For NPCs, scripts,
        /// and debug. The player path uses BeginInteract / CancelInteract.</summary>
        [ContextMenu("Harvest")]
        public void Harvest()
        {
            if (!CanHarvest) return;
            Complete();
        }

        //==================== LIFECYCLE =====================
        private void Update()
        {
            if (!isInteracting) return;

            progress = harvestDuration <= 0f
                ? 1f
                : Mathf.Min(1f, progress + Time.deltaTime / harvestDuration);

            OnProgress?.Invoke(progress);

            if (progress >= 1f) Complete();
        }

        //==================== PRIVATE =====================
        private void Complete()
        {
            isInteracting = false;
            isHarvested = true;
            progress = 1f;

            if (ResourceManager.Instance)
                ResourceManager.Instance.Add(resource, yieldAmount);

            OnHarvested?.Invoke();
            harvestedEvent?.Invoke();

            Destroy(gameObject, destroyDelay);
        }
    }
}

// ============================================================================
// Setup in a scene
//   1. Add this Harvestable component to the harvestable GameObject.
//      Assign a ResourceData asset, a yield amount, and a harvestDuration
//      that matches the object's feel (bush ~0.5s, tree ~1.5s, boulder ~4s).
//   2. Add a Focusable component (required by HarvestableUI / PlayerInteractor).
//   3. Add HarvestableUI for the world-space prompt.
//   4. Add InteractableRadial for the hold-progress ring.
//   5. (Optional) Hook particles / SFX to the harvestedEvent UnityEvent.
//   6. Make sure one ResourceManager exists somewhere in the scene.
//   7. Make sure the player has a PlayerInteractor with a RaycastSensor.
// ============================================================================
