using UnityEngine;

namespace Ludocore
{
    public class CoreDeathReporter : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;

        private void Reset()
        {
            if (!gameManager) gameManager = FindObjectOfType<GameManager>();
        }

        // Call this when the core is destroyed.
        public void DestroyCore()
        {
            gameManager?.ReportCoreDestroyed();
        }
    }
}