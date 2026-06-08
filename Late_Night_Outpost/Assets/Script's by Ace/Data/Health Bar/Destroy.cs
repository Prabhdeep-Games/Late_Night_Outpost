using UnityEngine;

namespace Ludocore
{
    [RequireComponent(typeof(Collider))]
    public class DamageZone : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private float damageAmount = 10f;

        [Tooltip("Layers that can be damaged by this zone.")]
        [SerializeField] private LayerMask damageLayers;

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

            // 1) Layer filter – skip anything not in damageLayers
            if ((damageLayers.value & (1 << other.gameObject.layer)) == 0) return;

            // 2) Find IDamageable on this object or its parents
            if (other.GetComponentInParent<IDamageable>() is { } damageable)
            {
                damageable.TakeDamage(damageAmount);
            }
        }
    }
}