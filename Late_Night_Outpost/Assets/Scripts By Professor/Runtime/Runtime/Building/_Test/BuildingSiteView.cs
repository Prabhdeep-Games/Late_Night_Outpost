using UnityEngine;
using TMPro;

namespace Ludocore.Test
{
    /// <summary>Presentation layer for a building site — prompt, marker, effects. (TEST)</summary>
    public class BuildingSiteViewTest : MonoBehaviour
    {
        //==================== CONFIG =====================
        [Header("References")]
        [Tooltip("The BuildingSite logic component.")]
        [SerializeField] private BuildingSiteTest site;

        [Tooltip("Sensor used to show/hide prompt on proximity.")]
        [SerializeField] private Sensor sensor;

        [Header("Prompt")]
        [Tooltip("World-space canvas for the interaction prompt.")]
        [SerializeField] private Canvas promptCanvas;

        [Tooltip("Text element showing build/upgrade prompt.")]
        [SerializeField] private TextMeshProUGUI promptText;

        [Header("Marker")]
        [Tooltip("Visual marker shown before first build (e.g. ground disc).")]
        [SerializeField] private GameObject marker;

        [Header("Effects")]
        [Tooltip("Particle system played on successful build.")]
        [SerializeField] private ParticleSystem buildEffect;

        [Header("Strings")]
        [SerializeField] private string buildString = "Press F to Build";
        [SerializeField] private string upgradeString = "Press F to Upgrade";
        [SerializeField] private string failString = "Cannot build!";

        //==================== STATE =====================
        private bool _playerInRange;

        //==================== LIFECYCLE =====================
        private void OnEnable()
        {
            site.OnBuilt += HandleBuilt;
            site.OnBuildFailed += HandleBuildFailed;
        }

        private void OnDisable()
        {
            site.OnBuilt -= HandleBuilt;
            site.OnBuildFailed -= HandleBuildFailed;
        }

        private void Start()
        {
            SetPromptVisible(false);
        }

        private void Update()
        {
            bool inRange = sensor && sensor.HasDetections && !site.IsMaxLevel;

            if (inRange != _playerInRange)
            {
                _playerInRange = inRange;
                SetPromptVisible(_playerInRange);
            }

            // Billboard prompt toward camera
            if (_playerInRange && promptCanvas)
            {
                promptCanvas.transform.forward = Camera.main.transform.forward;
            }
        }

        //==================== PRIVATE =====================
        private void HandleBuilt(int level)
        {
            // Hide marker on first build
            if (level == 0 && marker) marker.SetActive(false);

            // Play build particles
            if (buildEffect) buildEffect.Play();

            // Trigger spawn animation on the new prefab
            var spawnView = GetComponentInChildren<BuildingSpawnViewTest>();
            if (spawnView) spawnView.Play();

            // Update prompt text
            if (site.IsMaxLevel)
            {
                SetPromptVisible(false);
            }
            else
            {
                UpdatePromptText(upgradeString);
            }
        }

        private void HandleBuildFailed()
        {
            UpdatePromptText(failString);
        }

        private void SetPromptVisible(bool visible)
        {
            if (!promptCanvas) return;
            promptCanvas.gameObject.SetActive(visible);

            if (visible)
            {
                UpdatePromptText(site.IsBuilt ? upgradeString : buildString);
            }
        }

        private void UpdatePromptText(string text)
        {
            if (promptText) promptText.text = text;
        }
    }
}
