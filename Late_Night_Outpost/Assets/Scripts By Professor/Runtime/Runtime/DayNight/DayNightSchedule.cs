// ============================================
// Day Night Schedule
// ============================================
// Raises a GameEvent at specific (day, hour) targets.
// One-shot per entry. Targets already in the past at OnEnable are skipped.
// Note: with dawn-rollover, hours 0..5 of Day N are the pre-dawn early morning
// while DayCount still reads N-1. Pick targets accordingly.
// ============================================

using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Ludocore
{
    public class DayNightSchedule : MonoBehaviour
    {
        [Serializable]
        public class Entry
        {
            [Tooltip("Day on which the event fires.")]
            [Min(1)] public int day = 1;

            [Tooltip("Hour at which the event fires (0..23).")]
            [Range(0, 23)] public int hour = 6;

            [Tooltip("Event raised when the schedule matches.")]
            [FormerlySerializedAs("channel")]
            public GameEvent gameEvent;
        }

        //==================== CONFIG =====================
        [Header("Source")]
        [Tooltip("Normalized time of day (0..1).")]
        [SerializeField] private FloatVariable timeOfDay;

        [Tooltip("Current day number.")]
        [SerializeField] private IntVariable dayCount;

        [Header("Schedule")]
        [Tooltip("One-shot events. Each fires the first frame current time reaches its target.")]
        [SerializeField] private Entry[] entries;

        //==================== STATE =====================
        private bool[] _fired;

        //==================== LIFECYCLE =====================
        private void OnEnable()
        {
            _fired = new bool[entries.Length];

            // Skip targets that are already in the past at start.
            int day = dayCount.Value;
            int hour = HourOf(timeOfDay.Value);
            for (int i = 0; i < entries.Length; i++)
            {
                Entry e = entries[i];
                if (day > e.day || (day == e.day && hour > e.hour))
                    _fired[i] = true;
            }
        }

        private void Update()
        {
            int day = dayCount.Value;
            int hour = HourOf(timeOfDay.Value);

            for (int i = 0; i < entries.Length; i++)
            {
                if (_fired[i]) continue;
                Entry e = entries[i];
                if (day > e.day || (day == e.day && hour >= e.hour))
                {
                    e.gameEvent.Raise();
                    _fired[i] = true;
                }
            }
        }

        //==================== PRIVATE =====================
        private static int HourOf(float timeOfDay) => Mathf.FloorToInt(timeOfDay * 24f);
    }
}
