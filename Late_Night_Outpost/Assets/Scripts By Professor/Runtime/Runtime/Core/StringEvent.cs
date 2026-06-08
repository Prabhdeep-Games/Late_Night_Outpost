// ============================================
// String Event
// ============================================
// Broadcast event with a single string payload (chat line, error code, etc.).
//
// USAGE: Create an asset (right-click → Create → Ludocore/Events/String Event),
//        rename it semantically (ChatLineEvent.asset, ErrorReportedEvent.asset),
//        and reference it from systems that raise or listen.
//
// NAMING: The class is generic; the ASSET NAME carries the meaning.
//         Never leave assets named "NewStringEvent" — rename immediately.
//
// FOR MULTI-FIELD PAYLOADS: copy ScheduledTimeEvent.cs as a template instead.
// ============================================

using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Ludocore
{
    [CreateAssetMenu(fileName = "NewStringEvent", menuName = "Ludocore/Events/String Event")]
    public class StringEvent : ScriptableObject
    {
        //==================== DEBUG =====================
        [Header("Debug")]
        [Tooltip("Log to console when this event is raised. Per-asset toggle.")]
        [SerializeField] private bool debugLog = false;

        //==================== DIAGNOSTICS =====================
        public int RaiseCount { get; private set; }
        public float LastRaiseTime { get; private set; } = -1f;
        public string LastValue { get; private set; } = "";
        public bool HasBeenRaised => LastRaiseTime >= 0f;

        //==================== OUTPUTS =====================
        private Action<string> _onRaised;

        /// <summary>Raised when Raise(string) is called. Subscribe in OnEnable, unsubscribe in OnDisable.</summary>
        public event Action<string> OnRaised
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
        public void Raise(string value)
        {
            RaiseCount++;
            LastRaiseTime = Time.realtimeSinceStartup;
            LastValue = value;
            if (debugLog)
                Debug.Log($"<color=#f75369>[Event]</color> {name} raised: \"{value}\"", this);
            _onRaised?.Invoke(value);
        }

        //==================== UNITY LIFECYCLE =====================
        private void Awake()
        {
            hideFlags = HideFlags.DontUnloadUnusedAsset;
        }

        private void OnEnable()
        {
            _onRaised = null;
            RaiseCount = 0;
            LastRaiseTime = -1f;
            LastValue = "";
#if UNITY_EDITOR
            _editorListeners.Clear();
#endif
        }

        //==================== EDITOR =====================
#if UNITY_EDITOR
        private readonly List<Object> _editorListeners = new();
        public IReadOnlyList<Object> EditorListeners => _editorListeners;
#endif
    }
}
