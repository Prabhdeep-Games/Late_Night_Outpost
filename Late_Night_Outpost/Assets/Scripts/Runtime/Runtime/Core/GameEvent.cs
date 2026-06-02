// ============================================
// Game Event
// ============================================
// Named broadcast asset. Any script calls Raise(); subscribers react.
//
// SUBSCRIBE: from code      → gameEvent.OnRaised += MyMethod (and -= in OnDisable)
//            from Inspector → drop a GameEventListener and wire its UnityEvent
//
// Toggle debugLog on the asset to see colorized console output every time
// this specific event fires (with the listening MonoBehaviour linked).
//
// The asset's inspector also shows live diagnostics during play:
// raise count, time since last raise, and the current listener list.
// ============================================

using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Ludocore
{
    [CreateAssetMenu(fileName = "NewGameEvent", menuName = "Ludocore/Events/Game Event")]
    public class GameEvent : ScriptableObject
    {
        //==================== DEBUG =====================
        [Header("Debug")]
        [Tooltip("Log to console when this event is raised. Per-asset toggle.")]
        [SerializeField] private bool debugLog = false;

        //==================== DIAGNOSTICS =====================
        // Runtime-only fields surfaced by the custom inspector during play.
        public int RaiseCount { get; private set; }
        public float LastRaiseTime { get; private set; } = -1f;
        public bool HasBeenRaised => LastRaiseTime >= 0f;

        //==================== OUTPUTS =====================
        private Action _onRaised;

        /// <summary>Raised when Raise() is called. Subscribe in OnEnable, unsubscribe in OnDisable.</summary>
        public event Action OnRaised
        {
            add
            {
                _onRaised += value;
#if UNITY_EDITOR
                if (value.Target is Object o && !_editorListeners.Contains(o))
                    _editorListeners.Add(o);
#endif
            }
            remove
            {
                _onRaised -= value;
#if UNITY_EDITOR
                if (value.Target is Object o) _editorListeners.Remove(o);
#endif
            }
        }

        //==================== PUBLIC API =====================
        public void Raise()
        {
            RaiseCount++;
            LastRaiseTime = Time.realtimeSinceStartup;
            if (debugLog)
                Debug.Log($"<color=#f75369>[Event]</color> {name} raised", this);
            _onRaised?.Invoke();
        }

        //==================== UNITY LIFECYCLE =====================
        private void Awake()
        {
            // Prevents Unity from unloading the SO when nothing temporarily references it.
            hideFlags = HideFlags.DontUnloadUnusedAsset;
        }

        private void OnEnable()
        {
            _onRaised = null; // clear stale subscribers from previous play session
            RaiseCount = 0;
            LastRaiseTime = -1f;
#if UNITY_EDITOR
            _editorListeners.Clear();
#endif
        }

        //==================== EDITOR =====================
#if UNITY_EDITOR
        private readonly List<Object> _editorListeners = new();

        /// <summary>Objects currently subscribed to OnRaised. Editor-only; for debugging.</summary>
        public IReadOnlyList<Object> EditorListeners => _editorListeners;
#endif
    }
}
