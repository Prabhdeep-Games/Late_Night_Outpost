using UnityEngine;

namespace Ludocore
{
    // TEMP: attach to Core, and call ApplyTestDamage from the inspector (context menu) or another script.
    public class CoreHealthDirectTester : MonoBehaviour
    {
        [SerializeField] private HealthSystem health;
        [SerializeField] private float damageAmount = 10f;

        private void Reset()
        {
            if (!health) health = GetComponent<HealthSystem>();
        }

        [ContextMenu("Apply Test Damage")]
        private void ApplyTestDamage()
        {
            if (!health)
            {
                Debug.LogError("CoreHealthDirectTester: No HealthSystem reference on Core.");
                return;
            }

            Debug.Log($"CoreHealthDirectTester: Before damage, health = {health.CurrentHealth}");
            health.TakeDamage(damageAmount);
            Debug.Log($"CoreHealthDirectTester: After damage, health = {health.CurrentHealth}");
        }
    }
}