// ============================================================================
// ResourceUI — Presentation layer
//
// The HUD row(s) showing how much of each resource the player has.
// Data-driven: add a ResourceData to the tracked list, a row appears at
// runtime. Subscribes to ResourceManager.OnChanged and updates the matching
// label whenever the amount changes.
//
// Does not track state, does not modify resources. It only reads and displays.
// ============================================================================

using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Ludocore
{
    /// <summary>HUD displaying resource amounts. Rows are spawned from the tracked list.</summary>
    public class ResourceUI : MonoBehaviour
    {
        //==================== CONFIG =====================
        [Header("Config")]
        [Tooltip("Resources to display. Order determines HUD order. Add a ResourceData here and a row appears at runtime.")]
        [SerializeField] private List<ResourceData> tracked = new();

        [Tooltip("Prefab containing a TMP_Text (any child). Instantiated once per tracked resource.")]
        [SerializeField] private GameObject rowPrefab;

        [Tooltip("Parent transform where rows are instantiated.")]
        [SerializeField] private Transform rowParent;

        [Tooltip("{0} = resource name, {1} = amount.")]
        [SerializeField] private string rowFormat = "{0}: {1}";

        //==================== STATE =====================
        private readonly Dictionary<ResourceData, TMP_Text> _labels = new();

        //==================== LIFECYCLE =====================
        private void Start()
        {
            SpawnRows();

            if (ResourceManager.Instance)
                ResourceManager.Instance.OnChanged += HandleChanged;
        }

        private void OnDestroy()
        {
            if (ResourceManager.Instance)
                ResourceManager.Instance.OnChanged -= HandleChanged;
        }

        //==================== PRIVATE =====================
        private void SpawnRows()
        {
            if (!rowPrefab) return;
            if (!rowParent) return;

            foreach (ResourceData r in tracked)
            {
                if (!r) continue;
                if (_labels.ContainsKey(r)) continue;

                GameObject row = Instantiate(rowPrefab, rowParent);
                row.name = $"Row_{r.DisplayName}";

                TMP_Text label = row.GetComponentInChildren<TMP_Text>();
                if (!label) continue;

                int startingAmount = ResourceManager.Instance
                    ? ResourceManager.Instance.Get(r)
                    : 0;

                label.text = string.Format(rowFormat, r.DisplayName, startingAmount);
                label.color = r.Color;
                _labels[r] = label;
            }
        }

        private void HandleChanged(ResourceData resource, int newAmount, int delta)
        {
            if (!_labels.TryGetValue(resource, out TMP_Text label)) return;

            label.text = string.Format(rowFormat, resource.DisplayName, newAmount);
            label.color = resource.Color;
        }
    }
}

// ============================================================================
// Setup in a scene
//   1. Create a screen-space Canvas with a vertical layout group (the rowParent).
//   2. Create a row prefab: a small GameObject containing at least one TMP_Text.
//   3. Add this ResourceUI component somewhere on the Canvas.
//   4. Wire rowPrefab and rowParent; drop the ResourceData assets you want to
//      display into the tracked list.
//   5. Press Play — one row appears per tracked resource, live-updating on
//      every harvest.
// ============================================================================
