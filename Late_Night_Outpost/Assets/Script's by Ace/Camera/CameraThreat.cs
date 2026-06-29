using UnityEngine;
using UnityEngine.UI;

namespace Ludocore
{
    /// <summary>
    /// Fades a full-screen vignette based on how close the nearest enemy is.
    /// Far = clear, near = dark.
    /// </summary>
    public class CameraThreatVision : MonoBehaviour
    {
        [Header("Config")]
        [Tooltip("Player transform used as the distance origin.")]
        [SerializeField] private Transform player;

        [Tooltip("Registry that tracks all live enemies.")]
        [SerializeField] private EnemyRegistry enemyRegistry;

        [Tooltip("Fullscreen UI Image used as the dark vignette overlay.")]
        [SerializeField] private Image vignetteImage;

        [Header("Distances")]
        [Tooltip("Distance at or closer than which the vignette is strongest.")]
        [SerializeField] private float nearDistance = 3f;

        [Tooltip("Distance at or beyond which the vignette is fully cleared.")]
        [SerializeField] private float farDistance = 20f;

        [Header("Intensity")]
        [Tooltip("Maximum alpha applied to the vignette when threat is highest.")]
        [Range(0f, 1f)]
        [SerializeField] private float maxAlpha = 0.5f;

        private void Reset()
        {
            // Try auto-wire if possible
            if (!player)
            {
                var playerObj = GameObject.FindWithTag("Player");
                if (playerObj) player = playerObj.transform;
            }

            if (!enemyRegistry)
            {
                enemyRegistry = Resources.Load<EnemyRegistry>("EnemyRegistry");
            }

            if (!vignetteImage)
            {
                vignetteImage = GetComponentInChildren<Image>();
            }
        }

        private void Update()
        {
            if (!player || !enemyRegistry || !vignetteImage)
            {
                return;
            }

            float nearest = GetNearestEnemyDistance();

            // If no enemies, clear vignette
            if (nearest == Mathf.Infinity)
            {
                SetAlpha(0f);
                return;
            }

            // Map distance to 0..1 (far = 0, near = 1)
            float t = Mathf.InverseLerp(farDistance, nearDistance, nearest);
            float alpha = Mathf.Clamp01(t) * maxAlpha;

            SetAlpha(alpha);
        }

        private float GetNearestEnemyDistance()
        {
            float nearest = Mathf.Infinity;

            int count = enemyRegistry.Count;
            for (int i = 0; i < count; i++)
            {
                Enemy enemy = enemyRegistry[i];
                if (!enemy) continue;

                float d = Vector3.Distance(player.position, enemy.transform.position);
                if (d < nearest)
                {
                    nearest = d;
                }
            }

            return nearest;
        }

        private void SetAlpha(float alpha)
        {
            Color c = vignetteImage.color;
            c.a = alpha;
            vignetteImage.color = c;
        }

        private void OnDrawGizmosSelected()
{
    if (!player) return;

    // Far radius (effect starts)
    Gizmos.color = new Color(0f, 0f, 1f, 0.15f);
    Gizmos.DrawSphere(player.position, farDistance);
    Gizmos.color = Color.red;
    Gizmos.DrawWireSphere(player.position, farDistance);

    // Near radius (effect strongest)
    Gizmos.color = new Color(1f, 0f, 0f, 0.15f);
    Gizmos.DrawSphere(player.position, nearDistance);
    Gizmos.color = Color.yellow;
    Gizmos.DrawWireSphere(player.position, nearDistance);
}

    }
}
