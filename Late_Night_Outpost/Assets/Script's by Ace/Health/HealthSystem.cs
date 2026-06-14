// ============================================================================
// HealthSystem — Logic layer
//
// Tracks current health for any entity (player, enemy, building). Implements
// IDamageable so damage sources don't need to know what they're hitting.
//
// Dual outputs: every event exposes both a C# Action (for code wiring) and a
// UnityEvent (for Inspector wiring by students). Both fire together.
//
// Damage cooldown is global per-entity (not per-attacker) — KISS.
// ============================================================================

using System;
using UnityEngine;
using UnityEngine.Events;

namespace Ludocore
{
    /// <summary>Tracks health and processes damage and healing for an entity.</summary>
    public class HealthSystem : MonoBehaviour, IDamageable
    {
        //==================== CONFIG =====================
        [Header("Config")]
        [Tooltip("Data asset defining max health and damage cooldown")]
        [SerializeField] private HealthData data;

        //==================== STATE =====================
        [Header("Debug")]
        [ReadOnly, SerializeField] private float currentHealth;
        [ReadOnly, SerializeField] private bool isDead;

        private float _lastDamageTime = -Mathf.Infinity;

        public float CurrentHealth => currentHealth;
        public float MaxHealth     => data.MaxHealth;
        public float HealthRatio   => currentHealth / data.MaxHealth;
        public bool  IsDead        => isDead;

        //==================== OUTPUTS =====================
        public event Action<float> OnDamaged;
        public event Action<float> OnHealed;
        public event Action        OnDied;

        [Header("Events")]
        [Tooltip("Fired when this entity takes damage. Passes amount applied.")]
        [SerializeField] private UnityEvent<float> damagedEvent;

        [Tooltip("Fired when this entity is healed. Passes amount applied.")]
        [SerializeField] private UnityEvent<float> healedEvent;

        [Tooltip("Fired once when this entity dies.")]
        [SerializeField] private UnityEvent diedEvent;

        //==================== LIFECYCLE =====================
        private void Awake()
        {
            currentHealth = data.MaxHealth;
        }

        //==================== INPUTS =====================
        /// <summary>Apply damage. Ignored if dead, non-positive, or within cooldown.</summary>
        public void TakeDamage(float amount)
        {
            if (isDead) return;
            if (amount <= 0f) return;
            if (Time.time - _lastDamageTime < data.DamageCooldown) return;

            _lastDamageTime = Time.time;
            currentHealth = Mathf.Max(0f, currentHealth - amount);

            OnDamaged?.Invoke(amount);
            damagedEvent?.Invoke(amount);

            if (currentHealth <= 0f) Die();
        }

        /// <summary>Restore health. Ignored if dead or non-positive.</summary>
        public void Heal(float amount)
        {
            if (isDead) return;
            if (amount <= 0f) return;

            currentHealth = Mathf.Min(data.MaxHealth, currentHealth + amount);

            OnHealed?.Invoke(amount);
            healedEvent?.Invoke(amount);
        }

        [ContextMenu("Take 10 Damage")]
        private void Debug_Take10() => TakeDamage(10f);

        [ContextMenu("Heal 10")]
        private void Debug_Heal10() => Heal(10f);

        [ContextMenu("Kill")]
        private void Debug_Kill() => TakeDamage(currentHealth);

        //==================== PRIVATE =====================
        private void Die()
        {
            isDead = true;
            OnDied?.Invoke();
            diedEvent?.Invoke();
        }
    }
}
