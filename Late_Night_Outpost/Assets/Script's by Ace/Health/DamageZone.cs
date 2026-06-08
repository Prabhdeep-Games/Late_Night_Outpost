// ============================================================================
// DamageZone — Logic layer
//
// Damages any IDamageable that an attached Sensor reports as detected. Knows
// nothing about HOW the sensor detects — collider trigger, proximity, gaze,
// raycast. Swap the sensor type on the prefab to change the mechanism.
//
// The active window (windup → active → recovery) is gated by toggling
// DamageZone.enabled. On enable, anything currently in the sensor's signal
// list is hit immediately — the slash "lands" on whoever's already inside.
//
// hitOncePerTarget defaults true so lingering or scaling zones damage each
// target on first contact, not every frame.
//
// Self-hit prevention lives on the source sensor — configure its requiredTags
// to exclude the caster's tag so the caster's body never registers as a target.
// ============================================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Ludocore
{
    /// <summary>Damages IDamageables that an attached Sensor reports as detected.</summary>
    public class DamageArea: MonoBehaviour
    {
        //==================== CONFIG =====================
        [Header("Config")]
        [Tooltip("Sensor producing the detection signals. Swap the type (TriggerSensor, ProximitySensor, GazeSensor, RaycastSensor) to change how detection works.")]
        [SerializeField] private Sensor source;

        [Tooltip("Damage applied to each target on contact.")]
        [Min(0f)]
        [SerializeField] private float damage = 10f;

        [Tooltip("If true, each target is damaged only once for the lifetime of this zone. Use for lingering or scaling AOEs.")]
        [SerializeField] private bool hitOncePerTarget = true;

        //==================== STATE =====================
        [Header("Debug")]
        [ReadOnly, SerializeField] private int hitCount;

        private readonly HashSet<IDamageable> _alreadyHit = new();

        //==================== OUTPUTS =====================
        public event Action<IDamageable> OnHit;

        [Header("Events")]
        [Tooltip("Fired each time this zone damages something. Passes the GameObject that was hit.")]
        [SerializeField] private UnityEvent<GameObject> hitEvent;

        //==================== LIFECYCLE =====================
        private void OnEnable()
        {
            source.OnSignalAdded += HandleSignal;

            // Anything already inside the sensor counts as "entered" at the
            // moment we go active — the slash lands on whoever's there.
            var signals = source.Signals;
            for (int i = 0; i < signals.Count; i++) HandleSignal(signals[i]);
        }

        private void OnDisable()
        {
            source.OnSignalAdded -= HandleSignal;
        }

        //==================== PRIVATE =====================
        private void HandleSignal(Signal s)
        {
            if (!s.Object) return;

            var target = s.Object.GetComponentInParent<IDamageable>();
            if (target == null) return;

            if (hitOncePerTarget && !_alreadyHit.Add(target)) return;

            target.TakeDamage(damage);
            hitCount++;

            OnHit?.Invoke(target);
            hitEvent?.Invoke(s.Object);
        }
    }
}
