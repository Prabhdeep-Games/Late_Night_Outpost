using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ludocore
{
    public class GameManager : MonoBehaviour
    {
        [Header("State")]
        [SerializeField] private bool isGameOver;

        public bool IsGameOver => isGameOver;

        public void ReportPlayerDied()
        {
            if (isGameOver) return;
            Debug.Log("GAME OVER: Player died.");
            HandleGameOver();
        }

        public void ReportCoreDestroyed()
        {
            if (isGameOver) return;
            Debug.Log("GAME OVER: Core destroyed.");
            HandleGameOver();
        }

        private void HandleGameOver()
        {
            isGameOver = true;
            Time.timeScale = 0f;
            StartCoroutine(RestartAfterDelay(1.5f));
        }

        private System.Collections.IEnumerator RestartAfterDelay(float delay)
        {
            float elapsed = 0f;
            while (elapsed < delay)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            Time.timeScale = 1f;
            var current = SceneManager.GetActiveScene();
            SceneManager.LoadScene(current.buildIndex);
        }
    }
}