// ============================================
// Day Night Data
// ============================================
// Cycle length and phase thresholds for the day/night system.
// ============================================

using UnityEngine;

namespace Ludocore
{
    [CreateAssetMenu(fileName = "NewDayNightData", menuName = "Ludocore/Day Night Data")]
    public class DayNightData : ScriptableObject
    {
        [Header("Cycle")]
        [Tooltip("Length of one full day-night cycle in seconds.")]
        [Min(0.01f)]
        [SerializeField] private float cycleSeconds = 120f;

        [Header("Phase Thresholds")]
        [Tooltip("TimeOfDay where Night ends and Day begins (0..1).")]
        [Range(0f, 1f)]
        [SerializeField] private float dawnThreshold = 0.25f;

        [Tooltip("TimeOfDay where Day ends and Night begins (0..1).")]
        [Range(0f, 1f)]
        [SerializeField] private float duskThreshold = 0.75f;

        public float CycleSeconds => cycleSeconds;
        public float DawnThreshold => dawnThreshold;
        public float DuskThreshold => duskThreshold;
    }
}
