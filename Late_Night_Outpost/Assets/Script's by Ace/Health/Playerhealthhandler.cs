using UnityEngine;

namespace Ludocore
{
    /// <summary>Handles player-specific responses to death (respawn, penalties).</summary>
    public class PlayerHealthHandler : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private HealthSystem health;

        private void Reset()
        {
            if (!health) health = GetComponent<HealthSystem>();
        }

        private void OnEnable()
        {
            if (!health) return;
            health.OnDied += HandlePlayerDeath;
        }

        private void OnDisable()
        {
            if (!health) return;
            health.OnDied -= HandlePlayerDeath;
        }

        private void HandlePlayerDeath()
        {
            Debug.Log("Player died.");

            // TODO (later): respawn logic or penalties.
            // For the prototype, you can also call FindObjectOfType<GameManager>() and trigger a lose, if desired.
        }
    }
}