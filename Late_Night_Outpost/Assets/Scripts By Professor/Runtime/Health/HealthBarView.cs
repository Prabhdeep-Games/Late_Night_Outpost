// ============================================================================
// HealthBarView — Presentation layer
//
// Generic world-space HP bar. Lives as a child of any entity with a
// HealthSystem. Updates only when health changes (event-driven).
//
// Wiring is explicit: drag the entity's HealthSystem into the Health field
// when authoring the prefab. Same wiring discipline as every other view in
// the project — the dependency is visible in the Inspector.
//
// Camera-facing is handled by a separate billboard component. Per-entity
// flair (hit flash, death VFX, hurt sound) lives on its own scripts. This
// view does ONE thing: drive the slider from the HealthSystem.
// ============================================================================

using UnityEngine;
using UnityEngine.UI;

namespace Ludocore
{
    /// <summary>Generic world-space HP bar driven by a Slider.</summary>
    public class HealthBarView : MonoBehaviour
    {
        //==================== CONFIG =====================
        [Header("Config")]
        [Tooltip("HealthSystem to follow. Drag the entity's HealthSystem here when authoring the prefab.")]
        [SerializeField] private HealthSystem health;

        [Tooltip("Slider used to display the HP. value = current HP, maxValue = MaxHealth.")]
        [SerializeField] private Slider slider;

        [Tooltip("Optional. The Canvas (or any GameObject) toggled off when the bar should hide.")]
        [SerializeField] private GameObject barRoot;

        [Tooltip("Hide the bar while health is full.")]
        [SerializeField] private bool hideWhenFull = true;

        [Tooltip("Hide the bar when the entity dies.")]
        [SerializeField] private bool hideOnDeath = true;

        //==================== LIFECYCLE =====================
        private void OnEnable()
        {
            health.OnDamaged += HandleHealthChanged;
            health.OnHealed  += HandleHealthChanged;
            health.OnDied    += HandleDied;
        }

        // Start runs after all Awakes — guarantees HealthSystem has initialized
        // currentHealth before we read it for the first paint.
        private void Start()
        {
            Refresh();
        }

        private void OnDisable()
        {
            health.OnDamaged -= HandleHealthChanged;
            health.OnHealed  -= HandleHealthChanged;
            health.OnDied    -= HandleDied;
        }

        //==================== PRIVATE =====================
        private void HandleHealthChanged(float _) => Refresh();

        private void HandleDied()
        {
            if (hideOnDeath) SetVisible(false);
        }

        private void Refresh()
        {
            slider.maxValue = health.MaxHealth;
            slider.value    = health.CurrentHealth;

            if (hideWhenFull) SetVisible(health.CurrentHealth < health.MaxHealth);
        }

        private void SetVisible(bool visible)
        {
            if (barRoot) barRoot.SetActive(visible);
        }
    }
}
