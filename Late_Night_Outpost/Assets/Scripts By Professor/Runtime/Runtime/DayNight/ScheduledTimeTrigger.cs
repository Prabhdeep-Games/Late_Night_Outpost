// ============================================
// Scheduled Time Trigger
// ============================================
// PURPOSE: Watches the day/hour clock and raises a ScheduledTimeEvent when
//          the current time reaches the event's own Day/Hour target. One-shot:
//          fires once and stops. Targets already in the past at OnEnable are
//          skipped.
// USAGE:   Add to a GameObject. Assign timeOfDay + dayCount (the SO variables
//          the day/night controller writes to). Assign the event — its own
//          Day and Hour fields are the trigger target, so configure those on
//          the event asset itself.
// NOTE:    With dawn-rollover, hours 0..5 of Day N are the pre-dawn early
//          morning while dayCount still reads N-1. Pick targets accordingly.
// ============================================

using UnityEngine;

namespace Ludocore
{
    public class ScheduledTimeTrigger : MonoBehaviour
    {
        //==================== SOURCE =====================
        [Header("Source")]
        [Tooltip("Normalized time of day (0..1). Written by the day/night controller.")]
        [SerializeField] private FloatVariable timeOfDay;

        [Tooltip("Current day number. Written by the day/night controller.")]
        [SerializeField] private IntVariable dayCount;

        //==================== EVENT =====================
        [Header("Event")]
        [Tooltip("Raised once when current time reaches the event's Day/Hour target.")]
        [SerializeField] private ScheduledTimeEvent gameEvent;

        //==================== STATE =====================
        private bool _fired;

        //==================== LIFECYCLE =====================
        private void OnEnable()
        {
            // Skip if we're already past the target at start.
            int day = dayCount.Value;
            float hour = HourOf(timeOfDay.Value);
            _fired = day > gameEvent.Day || (day == gameEvent.Day && hour > gameEvent.Hour);
        }

        private void Update()
        {
            if (_fired) return;

            int day = dayCount.Value;
            float hour = HourOf(timeOfDay.Value);
            if (day > gameEvent.Day || (day == gameEvent.Day && hour >= gameEvent.Hour))
            {
                gameEvent.Raise();
                _fired = true;
            }
        }

        //==================== PRIVATE =====================
        private static float HourOf(float t) => t * 24f;
    }
}
