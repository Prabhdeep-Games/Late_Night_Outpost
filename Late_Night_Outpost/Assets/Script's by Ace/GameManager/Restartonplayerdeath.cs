using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ludocore
{
    public class RestartOnPlayerDeath : MonoBehaviour
    {
        [SerializeField] private HealthSystem health;
        [SerializeField] private bool hasRestarted;

        private void Reset()
        {
            // Auto‑grab HealthSystem from the same object
            if (!health) health = GetComponent<HealthSystem>();
        }

        private void Update()
        {
            if (hasRestarted) return;
            if (!health) return;

            // Change "CurrentHealth" to match your exact field/property name
            if (health.CurrentHealth <= 0f)
            {
                hasRestarted = true;
                RestartScene();
            }
        }

        private void RestartScene()
        {
            var current = SceneManager.GetActiveScene();
            SceneManager.LoadScene(current.buildIndex); // reloads current scene
        }
    }
}