using UnityEngine;

namespace Ludocore
{
    /// <summary>
    /// Simple trigger that applies damage to any IDamageable entering or staying inside.
    /// Great for testing HealthSystem and lose conditions.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class DamageZone : MonoBehaviour
    {
        [Header("Config")]
        [Tooltip("Damage applied each hit.")]
        [SerializeField] private float damageAmount = 10f;

        [Tooltip("If true, damage is applied every time the object stays inside this zone (per physics step).")]
        [SerializeField] private bool damageOnStay = false;

        private void Reset()
        {
            Collider col = GetComponent<Collider>();
            col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            TryDamage(other);
        }

        private void OnTriggerStay(Collider other)
        {
            if (!damageOnStay) return;
            TryDamage(other);
        }

        private void TryDamage(Collider other)
        {
            if (damageAmount <= 0f) return;

            // Look up the hierarchy so it works when the collider is on a child
            if (other.GetComponentInParent<IDamageable>() is { } damageable)
            {
                damageable.TakeDamage(damageAmount);
            }
        }
    }
}