// ============================================
// Scheduled Time Event
// ============================================
// PURPOSE: Event asset that carries Day and Hour values. Set them in the
//          Inspector, then click "Raise" via the asset's gear menu (or call
//          Raise() from code). Listeners read Day and Hour straight off the
//          assigned asset.
//
// PATTERN: This is the template for any typed event with data.
//          To make a new typed event (e.g. DamageEvent with int amount):
//            1. Copy this file, rename the class.
//            2. Replace the data fields with whatever the event carries.
//            3. Keep the lifecycle bits (Awake hideFlags + OnEnable clear).
//          The pattern is intentionally minimal — no generics, no inheritance.
//          Each typed event is one self-contained file you can read in 30s.
// ============================================

using System;
using UnityEngine;

namespace Ludocore
{
    [CreateAssetMenu(fileName = "NewScheduledTimeEvent", menuName = "Ludocore/Events/Scheduled Time Event")]
    public class ScheduledTimeEvent : ScriptableObject
    {
        //==================== DATA =====================
        [Header("Data")]
        [Tooltip("Day this event represents.")]
        [Min(1)] public int Day = 1;

        [Tooltip("Hour this event represents (0..23).")]
        [Range(0, 23)] public float Hour = 6f;

        //==================== EVENT =====================
        public event Action OnRaised;

        [ContextMenu("Raise")]
        public void Raise() => OnRaised?.Invoke();

        //==================== LIFECYCLE =====================
        // Prevents Unity from unloading the SO when nothing temporarily references it.
        private void Awake() => hideFlags = HideFlags.DontUnloadUnusedAsset;

        // Clears stale subscribers carried over from a previous play session.
        private void OnEnable() => OnRaised = null;
    }
}
