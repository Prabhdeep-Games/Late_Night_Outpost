// ============================================
// Day Night Controller
// ============================================
// Advances TimeOfDay each frame, increments DayCount at dawn,
// raises OnDawn / OnDusk / OnNewDay on phase transitions.
// ============================================

using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Ludocore
{
    public enum DayPhase { Night, Day }

    public class DayNightController : MonoBehaviour
    {
        //==================== CONFIG =====================
        [Header("Config")]
        [Tooltip("Cycle length and phase thresholds.")]
        [SerializeField] private DayNightData data;

        [Tooltip("Start the cycle automatically when this object is enabled.")]
        [FormerlySerializedAs("autoStart")]
        [SerializeField] private bool autoPlay = true;

        [Header("Variables")]
        [Tooltip("Normalized time of day (0..1). Written by this controller.")]
        [SerializeField] private FloatVariable timeOfDay;

        [Tooltip("Current day number. Incremented each dawn.")]
        [SerializeField] private IntVariable dayCount;

        [Header("Events")]
        [Tooltip("Raised when phase transitions Night -> Day.")]
        [SerializeField] private GameEvent onDawn;

        [Tooltip("Raised when phase transitions Day -> Night.")]
        [SerializeField] private GameEvent onDusk;

        [Tooltip("Raised when DayCount increments (always at dawn).")]
        [SerializeField] private GameEvent onNewDay;

        //==================== STATE =====================
        [Header("Debug")]
        [ReadOnly, SerializeField] private DayPhase currentPhase;
        [ReadOnly, SerializeField] private bool isRunning;

        public DayPhase CurrentPhase => currentPhase;
        public bool IsRunning => isRunning;

        //==================== OUTPUTS =====================
        public event Action<DayPhase> OnPhaseChanged;

        [Header("Events")]
        [Tooltip("Invoked when phase changes, passes the new phase.")]
        [SerializeField] private UnityEvent<DayPhase> phaseChangedEvent;

        //==================== LIFECYCLE =====================
        private void OnEnable()
        {
            currentPhase = ComputePhase(timeOfDay.Value);
            if (autoPlay) isRunning = true;
        }

        private void Update()
        {
            if (!isRunning) return;
            Tick(Time.deltaTime);
        }

        //==================== INPUTS =====================
        /// <summary>Advance the cycle by dt seconds.</summary>
        public void Tick(float dt)
        {
            timeOfDay.Value = Mathf.Repeat(timeOfDay.Value + dt / data.CycleSeconds, 1f);

            DayPhase next = ComputePhase(timeOfDay.Value);
            if (next != currentPhase) TransitionTo(next);
        }

        [ContextMenu("Run")]
        public void Run() => isRunning = true;

        [ContextMenu("Pause")]
        public void Pause() => isRunning = false;

        //==================== PRIVATE =====================
        private DayPhase ComputePhase(float t)
            => (t >= data.DawnThreshold && t < data.DuskThreshold) ? DayPhase.Day : DayPhase.Night;

        private void TransitionTo(DayPhase next)
        {
            currentPhase = next;

            if (next == DayPhase.Day)
            {
                onDawn.Raise();
                dayCount.Increment();
                onNewDay.Raise();
            }
            else
            {
                onDusk.Raise();
            }

            OnPhaseChanged?.Invoke(next);
            phaseChangedEvent?.Invoke(next);
        }
    }
}
