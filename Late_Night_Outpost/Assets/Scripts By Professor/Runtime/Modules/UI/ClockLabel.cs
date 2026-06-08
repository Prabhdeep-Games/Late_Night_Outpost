// ============================================
// Clock Label
// ============================================
// Binder: shows TimeOfDay (and optionally DayCount) as a formatted clock.
// Format tokens: {0} = day, {1} = hour (00..23), {2} = minute (00..59).
// ============================================

using TMPro;
using UnityEngine;

namespace Ludocore
{
    public class ClockLabel : MonoBehaviour
    {
        //==================== CONFIG =====================
        [Header("Source")]
        [Tooltip("Normalized time of day (0..1).")]
        [SerializeField] private FloatVariable timeOfDay;

        [Tooltip("Day counter. Optional — leave unassigned to show clock only.")]
        [SerializeField] private IntVariable dayCount;

        [Header("Display")]
        [Tooltip("Text component to update.")]
        [SerializeField] private TMP_Text label;

        [Tooltip("Format string. {0}=day, {1}=hour, {2}=minute. " +
                 "Examples: 'Day {0}, {1:00}:{2:00}', '{1:00}:{2:00}'.")]
        [SerializeField] private string format = "Day {0}, {1:00}:{2:00}";

        //==================== LIFECYCLE =====================
        private void OnEnable()
        {
            timeOfDay.OnChanged += OnTimeChanged;
            if (dayCount is not null) dayCount.OnChanged += OnDayChanged;
            Refresh();
        }

        private void OnDisable()
        {
            timeOfDay.OnChanged -= OnTimeChanged;
            if (dayCount is not null) dayCount.OnChanged -= OnDayChanged;
        }

        //==================== PRIVATE =====================
        private void OnTimeChanged(float _) => Refresh();
        private void OnDayChanged(int _) => Refresh();

        private void Refresh()
        {
            float t = timeOfDay.Value;
            int totalMinutes = Mathf.FloorToInt(t * 24f * 60f);
            int hour = (totalMinutes / 60) % 24;
            int minute = totalMinutes % 60;
            int day = dayCount is not null ? dayCount.Value : 0;
            label.text = string.Format(format, day, hour, minute);
        }
    }
}
