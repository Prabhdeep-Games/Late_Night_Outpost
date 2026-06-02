// ============================================================================
// HarvestableUI — Presentation layer
//
// A world-space prompt attached to a harvestable. Shows the prompt when the
// player focuses this object, hides it when focus leaves or the harvest
// succeeds.
//
// Dependencies are explicit and minimal:
//   Focusable (sibling)  — WHEN to show (OnFocused / OnUnfocused events)
//   Harvestable (sibling) — WHAT to show (resource name, yield)
//
// Two small, independent reasons to talk to two different components.
// ============================================================================

using TMPro;
using UnityEngine;

namespace Ludocore
{
    /// <summary>Shows / hides a world-space prompt based on sibling Focusable events.</summary>
    [RequireComponent(typeof(Focusable))]
    [RequireComponent(typeof(Harvestable))]
    public class HarvestableUI : MonoBehaviour
    {
        //==================== CONFIG =====================
        [Header("Config")]
        [Tooltip("Root GameObject of the world-space prompt. Shown when focused.")]
        [SerializeField] private GameObject promptRoot;

        [Tooltip("Label that receives the formatted prompt text.")]
        [SerializeField] private TMP_Text promptLabel;

        [Tooltip("{0} = resource name, {1} = yield amount. [E] is appended automatically when the harvest is available.")]
        [SerializeField] private string promptFormat = "Harvest {0} (+{1})";

        //==================== STATE =====================
        private Focusable _focusable;
        private Harvestable _harvestable;

        //==================== LIFECYCLE =====================
        private void Awake()
        {
            _focusable = GetComponent<Focusable>();
            _harvestable = GetComponent<Harvestable>();
            if (promptRoot) promptRoot.SetActive(false);
        }

        private void OnEnable()
        {
            _focusable.OnFocused += ShowPrompt;
            _focusable.OnUnfocused += HidePrompt;
            _harvestable.OnHarvested += HidePrompt;
        }

        private void OnDisable()
        {
            _focusable.OnFocused -= ShowPrompt;
            _focusable.OnUnfocused -= HidePrompt;
            _harvestable.OnHarvested -= HidePrompt;
        }

        //==================== PRIVATE =====================
        private void ShowPrompt()
        {
            if (!promptRoot) return;
            promptRoot.SetActive(true);

            if (!promptLabel) return;
            if (!_harvestable.Resource) return;

            string text = string.Format(promptFormat,
                _harvestable.Resource.DisplayName,
                _harvestable.Yield);
            if (_harvestable.CanHarvest) text += " [hold E]";

            promptLabel.text = text;
            promptLabel.color = _harvestable.Resource.Color;
        }

        private void HidePrompt()
        {
            if (promptRoot) promptRoot.SetActive(false);
        }
    }
}

// ============================================================================
// Setup in a scene
//   1. Requires sibling Harvestable and Focusable components.
//   2. Add a child world-space Canvas with a TMP_Text centered above the
//      object (facing the camera).
//   3. Add this HarvestableUI. Wire promptRoot (the Canvas) and promptLabel
//      (the TMP_Text).
//   4. Adjust promptFormat if you want different wording. The " [E]" suffix
//      is appended automatically when CanHarvest is true.
// ============================================================================
