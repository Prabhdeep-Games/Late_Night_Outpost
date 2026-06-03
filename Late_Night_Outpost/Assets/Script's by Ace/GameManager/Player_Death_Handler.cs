using UnityEngine;

namespace Ludocore
{
    public class PlayerDeathReporter : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;

        private void Reset()
        {
            if (!gameManager) gameManager = FindObjectOfType<GameManager>();
        }

        [ContextMenu("Test Player Death")]
        public void TestPlayerDeath()
        {
            if (!gameManager)
            {
                Debug.LogError("PlayerDeathReporter: No GameManager reference.");
                return;
            }

            gameManager.ReportPlayerDied();
        }

        // Call this from your actual health/death logic.
        public void OnDied()
        {
            gameManager?.ReportPlayerDied();
        }
    }
}