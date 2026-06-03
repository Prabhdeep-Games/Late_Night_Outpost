using UnityEngine;

namespace Ludocore
{
    public class CoreHealthHandler : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private HealthSystem health;
        [SerializeField] private GameManager gameManager;

        private void Reset()
        {
            if (!health) health = GetComponent<HealthSystem>();
            if (!gameManager) gameManager = FindObjectOfType<GameManager>();
        }

        private void OnEnable()
        {
            if (!health) return;
            health.OnDied += HandleCoreDestroyed;
        }

        private void OnDisable()
        {
            if (!health) return;
            health.OnDied -= HandleCoreDestroyed;
        }

        private void HandleCoreDestroyed()
        {
            // OLD:
            // gameManager?.HandleCoreDestroyed();

            // NEW:
            gameManager?.ReportCoreDestroyed();
        }
    }
}