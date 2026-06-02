// ============================================================================
// BuildingSiteView — Presentation layer
//
// Visual feedback for a build site:
//   • Particles toggled on focus
//   • World-space prompt (verb + name + cost + conditional [E] suffix)
//   • Ghost preview of the NEXT-tier prefab on focus
//
// The ghost is created from BuildingData.Prefab via GhostPreview — the same
// helper PlayerBuild uses for the held-builder flow. Because the site walks
// an upgrade chain, the ghost is rebuilt each time the chain advances so it
// always previews whatever the next Build() will spawn (or hides itself once
// the chain is exhausted).
//
// The emerge-from-below animation now lives on the building prefab itself
// (BuildingEmerge component). Each spawned tier brings its own intro — no
// per-piece authoring on the site.
//
// Dependencies (sibling):
//   Focusable    — WHEN to show ghost / prompt (OnFocused / OnUnfocused)
//   BuildingSite — WHAT to show (next-tier name, cost, prefab for the ghost)
// ============================================================================

using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace Ludocore
{
    /// <summary>Presentation for a build site — particles, prompt, ghost preview.</summary>
    [RequireComponent(typeof(Focusable))]
    [RequireComponent(typeof(BuildingSite))]
    public class BuildingSiteView : MonoBehaviour
    {
        //==================== CONFIG =====================
        [Header("Ghost")]
        [Tooltip("Material applied to the ghost clone (typically a blue transparent shader).")]
        [SerializeField] private Material ghostMaterial;

        [Header("Particles")]
        [Tooltip("Particle system played on focus. Child particle systems play automatically.")]
        [SerializeField] private ParticleSystem focusParticles;

        [Header("Canvas")]
        [Tooltip("Canvas containing the prompt.")]
        [SerializeField] private Canvas promptCanvas;

        [Tooltip("Label that receives the formatted prompt text.")]
        [SerializeField] private TextMeshProUGUI promptLabel;

        [Tooltip("Verb word shown before the building name when the plot is empty (first build).")]
        [SerializeField] private string buildVerb = "Build";

        [Tooltip("Verb phrase shown before the building name when an upgrade is available.")]
        [SerializeField] private string upgradeVerb = "Upgrade to";

        [Tooltip("Prefix for each cost line (placed before the amount).")]
        [SerializeField] private string costPrefix = "-";

        [Tooltip("Suffix appended when the build is available (typically the input key hint).")]
        [SerializeField] private string interactKey = "[E]";

        //==================== STATE =====================
        private Focusable _focusable;
        private BuildingSite _site;
        private GameObject _ghost;
        private BuildingData _ghostFor;

        //==================== LIFECYCLE =====================
        private void Awake()
        {
            _focusable = GetComponent<Focusable>();
            _site = GetComponent<BuildingSite>();
        }

        private void OnEnable()
        {
            _focusable.OnFocused += HandleFocused;
            _focusable.OnUnfocused += HandleUnfocused;
            _site.OnBuilt += HandleBuilt;
        }

        private void OnDisable()
        {
            _focusable.OnFocused -= HandleFocused;
            _focusable.OnUnfocused -= HandleUnfocused;
            _site.OnBuilt -= HandleBuilt;
        }

        private void Start()
        {
            HideParticles();
            HidePrompt();
            RebuildGhost(_site.Data);
        }

        //==================== HANDLERS =====================
        private void HandleFocused()
        {
            if (_site.IsMaxed) return; // chain exhausted — nothing to preview or prompt
            ShowParticles();
            ShowPrompt();
            // Rebuild on every focus so the ghost reflects any play-mode tuning
            // of spawnOffset / snap on the site (and re-snaps to current terrain).
            RebuildGhost(_site.Data);
            SetGhostActive(true);
        }

        private void HandleUnfocused()
        {
            HideParticles();
            HidePrompt();
            SetGhostActive(false);
        }

        private void HandleBuilt(BuildingData previous, BuildingData current)
        {
            // Chain advanced — ghost now needs to preview the new "next" tier (or vanish if maxed).
            RebuildGhost(_site.Data);
        }

        //==================== GHOST =====================
        private void RebuildGhost(BuildingData data)
        {
            if (_ghost) Destroy(_ghost);
            _ghost = null;
            _ghostFor = data;

            if (data == null || !data.Prefab) return;
            // Use the site's resolved spawn position (offset + ground snap) so the ghost
            // previews exactly where Build() will spawn the building.
            _ghost = GhostPreview.Create(data.Prefab, _site.SpawnPosition, transform.rotation, ghostMaterial, transform);
        }

        private void SetGhostActive(bool active)
        {
            if (_ghost) _ghost.SetActive(active);
        }

        //==================== PARTICLES =====================
        /// <summary>Play the focus particles (children included).</summary>
        public void ShowParticles()
        {
            if (focusParticles) focusParticles.Play();
        }

        /// <summary>Stop the focus particles (children included).</summary>
        public void HideParticles()
        {
            if (focusParticles) focusParticles.Stop();
        }

        //==================== CANVAS =====================
        /// <summary>Update the prompt text and show the canvas — verb + name + cost lines + optional [E] suffix.</summary>
        public void ShowPrompt()
        {
            if (!promptCanvas) return;

            if (promptLabel && _site.Data)
            {
                BuildingData data = _site.Data;
                string verb = _site.HasBuilding ? upgradeVerb : buildVerb;
                StringBuilder sb = new();
                sb.Append(verb).Append(' ').Append(data.BuildingName);

                IReadOnlyList<ResourceCost> costs = data.Costs;
                if (costs != null)
                {
                    for (int i = 0; i < costs.Count; i++)
                    {
                        ResourceCost c = costs[i];
                        if (c == null || !c.Data || c.Amount <= 0) continue;
                        sb.Append('\n').Append(costPrefix).Append(c.Amount).Append(' ').Append(c.Data.DisplayName);
                    }
                }

                if (_site.CanBuild) sb.Append(' ').Append(interactKey);
                promptLabel.text = sb.ToString();
            }

            promptCanvas.gameObject.SetActive(true);
        }

        /// <summary>Hide the canvas.</summary>
        public void HidePrompt()
        {
            if (!promptCanvas) return;
            promptCanvas.gameObject.SetActive(false);
        }
    }
}

// ============================================================================
// Setup in a scene
//   1. Requires sibling BuildingSite and Focusable components.
//   2. Add this BuildingSiteView. Assign:
//      - ghostMaterial (blue, transparent — applied to the ghost clone)
//      - focusParticles (optional)
//   3. Add a child world-space Canvas with a TextMeshProUGUI for the prompt.
//      Wire promptCanvas and promptLabel.
//   4. The ghost is created at Start from BuildingSite.Data.Prefab (the next
//      tier) and parented to this site. It appears on focus, hides on unfocus,
//      and is rebuilt every time the chain advances so each upgrade level
//      previews its own prefab.
//   5. The emerge-from-below animation lives on each building prefab itself
//      (BuildingEmerge component) — both this site and the held-prop Builder
//      flow get the same rise-up intro for free.
// ============================================================================
