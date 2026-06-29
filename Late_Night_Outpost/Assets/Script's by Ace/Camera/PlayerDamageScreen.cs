using UnityEngine;
using UnityEngine.UI;

namespace Ludocore
{
    /// <summary>
    /// Flashes a spiky damage overlay whenever the player takes damage.
    /// Driven directly by HealthSystem.OnDamaged.
    /// </summary>
    public class PlayerDamageScreen : MonoBehaviour
    {
        [Header("Config")]
        [Tooltip("HealthSystem on the player.")]
        [SerializeField] private HealthSystem health;

        [Tooltip("Fullscreen damage Image (spiky/red) to flash on hit.")]
        [SerializeField] private Image damageImage;

        [Tooltip("How quickly the damage image fades out after a hit.")]
        [SerializeField] private float fadeSpeed = 3f;

        [Tooltip("Target alpha when a hit occurs.")]
        [Range(0f, 1f)]
        [SerializeField] private float hitAlpha = 0.8f;

        private float _currentAlpha;

        private void Awake()
        {
            if (!health)
            {
                health = GetComponent<HealthSystem>();
            }
        }

        private void OnEnable()
        {
            if (health != null)
            {
                health.OnDamaged += HandleDamaged;
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.OnDamaged -= HandleDamaged;
            }
        }

        private void HandleDamaged(float amount)
        {
            // Only flash on positive damage
            if (amount <= 0f) return;

            _currentAlpha = hitAlpha;
            ApplyAlpha();
        }

        private void Update()
        {
            if (!damageImage) return;

            if (_currentAlpha > 0f)
            {
                _currentAlpha = Mathf.Max(0f, _currentAlpha - fadeSpeed * Time.deltaTime);
                ApplyAlpha();
            }
        }

        private void ApplyAlpha()
        {
            Color c = damageImage.color;
            c.a = _currentAlpha;
            damageImage.color = c;
        }
    }
}