// ============================================
// Scheduled Time Event Listener
// ============================================
// PURPOSE: Bridges a ScheduledTimeEvent to a UnityEvent response in the
//          Inspector. Methods wired to the response can read Day and Hour
//          directly from the assigned event asset.
// PATTERN: Use this as the template for any new typed-event listener: a
//          MonoBehaviour with a typed reference, += / -= subscription, and
//          a UnityEvent response.
// ============================================

using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

namespace Ludocore
{
    public class ScheduledTimeEventListener : MonoBehaviour
    {
        //==================== CONFIG =====================
        [Header("Config")]
        [Tooltip("The scheduled time event this listener subscribes to.")]
        [SerializeField] private ScheduledTimeEvent gameEvent;

        //==================== OUTPUTS =====================
        [Header("Events")]
        [Tooltip("Invoked when the event is raised. Read Day/Hour from the event asset.")]
        [SerializeField] private UnityEvent response;

        //==================== LIFECYCLE =====================
        private void OnEnable()  => gameEvent.OnRaised += response.Invoke;
        private void OnDisable() => gameEvent.OnRaised -= response.Invoke;
    }
}
