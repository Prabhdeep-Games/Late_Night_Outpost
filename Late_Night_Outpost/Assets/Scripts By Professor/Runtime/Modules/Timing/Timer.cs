using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Ludocore
{
    /// <summary>Ticks at a configurable interval and reports progress. Optional startDelay waits before the first tick begins.</summary>
    public class Timer : MonoBehaviour
    {
        //==================== CONFIG =====================
        [Header("Config")]
        [Tooltip("Duration of one tick in seconds")]
        [Min(0.01f)]
        [SerializeField] private float duration = 1f;

        [Tooltip("Number of ticks to run (0 = infinite)")]
        [Min(0)]
        [SerializeField] private int ticks = 1;

        [Tooltip("Start the timer automatically on enable")]
        [FormerlySerializedAs("autoStart")]
        [SerializeField] private bool autoPlay = true;

        [Tooltip("Seconds to wait after Run() before the timer actually begins ticking. OnStarted fires when the wait ends.")]
        [Min(0f)]
        [SerializeField] private float startDelay = 0f;

        //==================== STATE =====================
        [Header("Debug")]
        [ReadOnly, SerializeField] private bool isRunning;
        [ReadOnly, SerializeField] private int currentTick;

        private float _elapsed;
        private float _delayElapsed;
        private bool _hasStarted;
        private bool _isCompleted;

        public float Progress => Mathf.Clamp01(_elapsed / duration);
        public int CurrentTick => currentTick;
        public bool IsRunning => isRunning;
        public bool IsCompleted => _isCompleted;

        //==================== OUTPUTS =====================
        public event Action OnStarted;
        public event Action<float> OnProgress;
        public event Action OnTick;
        public event Action OnCompleted;

        [Header("Events")]
        [Tooltip("Fired once when the timer transitions from waiting to running (after startDelay).")]
        [SerializeField] private UnityEvent startedEvent;
        [Tooltip("Fired each frame with the current tick progress (0 to 1)")]
        [SerializeField] private UnityEvent<float> progressEvent;
        [Tooltip("Fired each time a tick completes")]
        [SerializeField] private UnityEvent tickEvent;
        [Tooltip("Fired when all ticks have completed")]
        [SerializeField] private UnityEvent completedEvent;

        //==================== LIFECYCLE =====================
        private void OnEnable()
        {
            if (autoPlay) Run();
        }

        private void Update()
        {
            if (!isRunning || _isCompleted) return;

            if (_delayElapsed < startDelay)
            {
                _delayElapsed += Time.deltaTime;
                if (_delayElapsed < startDelay) return;
                FireStartedIfNeeded();
            }

            _elapsed += Time.deltaTime;

            float progress = Progress;
            OnProgress?.Invoke(progress);
            progressEvent?.Invoke(progress);

            if (_elapsed < duration) return;

            _elapsed = 0f;
            currentTick++;
            OnTick?.Invoke();
            tickEvent?.Invoke();

            if (ticks <= 0 || currentTick < ticks) return;

            isRunning = false;
            _isCompleted = true;
            OnCompleted?.Invoke();
            completedEvent?.Invoke();
        }

        //==================== INPUTS =====================
        /// <summary>Start or resume the timer.</summary>
        [ContextMenu("Run")]
        public void Run()
        {
            if (_isCompleted) return;
            if (isRunning) return;
            isRunning = true;

            if (startDelay <= 0f || _delayElapsed >= startDelay)
                FireStartedIfNeeded();
        }

        /// <summary>Pause the timer without resetting.</summary>
        [ContextMenu("Stop")]
        public void Stop()
        {
            isRunning = false;
        }

        /// <summary>Reset all state to initial values.</summary>
        [ContextMenu("Clear")]
        public void Clear()
        {
            isRunning = false;
            _isCompleted = false;
            _elapsed = 0f;
            _delayElapsed = 0f;
            _hasStarted = false;
            currentTick = 0;
        }

        /// <summary>Reset and start immediately.</summary>
        [ContextMenu("Restart")]
        public void Restart()
        {
            Clear();
            Run();
        }

        //==================== PRIVATE =====================
        private void FireStartedIfNeeded()
        {
            if (_hasStarted) return;
            _hasStarted = true;
            OnStarted?.Invoke();
            startedEvent?.Invoke();
        }
    }
}
