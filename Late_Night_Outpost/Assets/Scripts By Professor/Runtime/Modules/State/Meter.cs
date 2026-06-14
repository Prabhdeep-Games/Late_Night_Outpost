using System;
using UnityEngine;
using UnityEngine.Events;

namespace Ludocore
{
    /// <summary>Leaky float accumulator (0..1) with two configurable thresholds. Fills while driven, drains otherwise. Used for stealth awareness, heat, reputation, charge, tension, etc.</summary>
    public class Meter : MonoBehaviour
    {
        //==================== CONFIG =====================
        [Header("Config")]
        [Tooltip("Rate of fill per second while driven (filling = true)"), Min(0f)]
        [SerializeField] private float fillRate = 0.5f;

        [Tooltip("Rate of drain per second when not driven"), Min(0f)]
        [SerializeField] private float drainRate = 0.3f;

        [Tooltip("Seconds to wait after fill stops before drain begins"), Min(0f)]
        [SerializeField] private float drainDelay = 1f;

        [Header("Thresholds")]
        [Tooltip("Lower threshold — OnLowReached fires when value crosses this going up")]
        [Range(0f, 1f)]
        [SerializeField] private float lowThreshold = 0.3f;

        [Tooltip("Upper threshold — OnHighReached fires when value crosses this going up")]
        [Range(0f, 1f)]
        [SerializeField] private float highThreshold = 1f;

        [Header("Initial")]
        [Tooltip("Value the meter starts at on enable (0..1)")]
        [Range(0f, 1f)]
        [SerializeField] private float initialValue;

        [Tooltip("Start filling automatically on enable")]
        [SerializeField] private bool autoFill;

        //==================== STATE =====================
        [Header("Debug")]
        [ReadOnly, SerializeField] private float value;
        [ReadOnly, SerializeField] private bool isFilling;
        [ReadOnly, SerializeField] private MeterLevel level;

        private float _timeSinceFillStopped;

        public float Value => value;
        public float Ratio => value;
        public bool IsFilling => isFilling;
        public MeterLevel Level => level;
        public bool IsBelow => level == MeterLevel.Below;
        public bool IsBetween => level == MeterLevel.Between;
        public bool IsAbove => level == MeterLevel.Above;

        //==================== OUTPUTS =====================
        public event Action<float> OnChanged;
        public event Action OnLowReached;
        public event Action OnHighReached;
        public event Action OnEmptied;

        [Header("Events")]
        [Tooltip("Fired whenever the value changes, passes the new value (0..1)")]
        [SerializeField] private UnityEvent<float> changedEvent;

        [Tooltip("Fired when the value crosses the lower threshold going up")]
        [SerializeField] private UnityEvent lowReachedEvent;

        [Tooltip("Fired when the value crosses the upper threshold going up")]
        [SerializeField] private UnityEvent highReachedEvent;

        [Tooltip("Fired when the value falls back below the lower threshold")]
        [SerializeField] private UnityEvent emptiedEvent;

        //==================== LIFECYCLE =====================
        private void OnEnable()
        {
            value = Mathf.Clamp01(initialValue);
            isFilling = autoFill;
            _timeSinceFillStopped = 0f;
            level = ComputeLevel();
        }

        private void Update()
        {
            float prev = value;

            if (isFilling)
            {
                value = Mathf.Min(1f, value + fillRate * Time.deltaTime);
                _timeSinceFillStopped = 0f;
            }
            else
            {
                _timeSinceFillStopped += Time.deltaTime;
                if (_timeSinceFillStopped >= drainDelay)
                    value = Mathf.Max(0f, value - drainRate * Time.deltaTime);
            }

            if (!Mathf.Approximately(prev, value))
            {
                OnChanged?.Invoke(value);
                changedEvent?.Invoke(value);
            }

            UpdateLevel();
        }

        //==================== INPUTS =====================
        /// <summary>Begin filling the meter.</summary>
        [ContextMenu("Start Filling")]
        public void StartFilling() => SetFilling(true);

        /// <summary>Stop filling — drain begins after drainDelay.</summary>
        [ContextMenu("Stop Filling")]
        public void StopFilling() => SetFilling(false);

        /// <summary>Toggle filling on/off. Useful for UnityEvent&lt;bool&gt; wiring.</summary>
        public void SetFilling(bool filling)
        {
            isFilling = filling;
            if (filling) _timeSinceFillStopped = 0f;
        }

        /// <summary>Add a one-time amount (positive or negative) to the meter.</summary>
        public void Add(float amount)
        {
            float prev = value;
            value = Mathf.Clamp01(value + amount);
            if (Mathf.Approximately(prev, value)) return;

            OnChanged?.Invoke(value);
            changedEvent?.Invoke(value);
            UpdateLevel();
        }

        /// <summary>Set the value directly (0..1).</summary>
        public void SetValue(float newValue)
        {
            float prev = value;
            value = Mathf.Clamp01(newValue);
            if (Mathf.Approximately(prev, value)) return;

            OnChanged?.Invoke(value);
            changedEvent?.Invoke(value);
            UpdateLevel();
        }

        /// <summary>Instantly fill to 1.0.</summary>
        [ContextMenu("Fill")]
        public void Fill() => SetValue(1f);

        /// <summary>Reset to initialValue and stop filling.</summary>
        [ContextMenu("Clear")]
        public void Clear()
        {
            value = Mathf.Clamp01(initialValue);
            isFilling = false;
            _timeSinceFillStopped = 0f;
            level = ComputeLevel();

            OnChanged?.Invoke(value);
            changedEvent?.Invoke(value);
        }

        //==================== PRIVATE =====================
        private void UpdateLevel()
        {
            MeterLevel newLevel = ComputeLevel();
            if (newLevel == level) return;

            MeterLevel old = level;
            level = newLevel;

            // Going up — crossed the lower threshold for the first time
            if (old == MeterLevel.Below && newLevel != MeterLevel.Below)
            {
                OnLowReached?.Invoke();
                lowReachedEvent?.Invoke();
            }

            // Going up — entered Above
            if (old != MeterLevel.Above && newLevel == MeterLevel.Above)
            {
                OnHighReached?.Invoke();
                highReachedEvent?.Invoke();
            }

            // Going down — fell back to Below
            if (old != MeterLevel.Below && newLevel == MeterLevel.Below)
            {
                OnEmptied?.Invoke();
                emptiedEvent?.Invoke();
            }
        }

        private MeterLevel ComputeLevel()
        {
            if (value >= highThreshold) return MeterLevel.Above;
            if (value >= lowThreshold) return MeterLevel.Between;
            return MeterLevel.Below;
        }
    }

    public enum MeterLevel { Below, Between, Above }
}
