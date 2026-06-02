// ============================================
// String Variable
// ============================================
// Reactive shared string. One writer sets Value; many readers observe via OnChanged.
//
// Authored value (initialValue) is what the designer sets in edit mode.
// Runtime value (runtimeValue) tracks live state during play.
// On exiting play, the authored value is restored from a snapshot — designer
// tweaks made during play are reverted (intentional; stop play to author).
// ============================================

using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Ludocore
{
    [CreateAssetMenu(fileName = "NewStringVariable", menuName = "Ludocore/Variables/String")]
    public class StringVariable : ScriptableObject
    {
        //==================== INITIAL VALUE =====================
        [Header("Initial Value")]
        [Tooltip("Authored value the designer sets. Edits during play are reverted on play exit.")]
        [TextArea(1, 4)]
        [SerializeField] private string initialValue = "";

        //==================== LIFECYCLE =====================
        [Header("Lifecycle")]
        [Tooltip("ON: runtime resets to initial value at play start; authored value is protected during play.\n" +
                 "OFF: variable is left untouched (manage manually).")]
        [SerializeField] private bool resetOnPlay = true;

        //==================== DEBUG =====================
        [Header("Debug")]
        [Tooltip("Log to console when Value changes. Per-asset toggle.")]
        [SerializeField] private bool debugLog = false;

        //==================== RUNTIME =====================
        [Header("Runtime State")]
        [Tooltip("Current runtime value. Visible during play.")]
        [ReadOnly, SerializeField] private string runtimeValue = "";

        //==================== INTERNAL =====================
        private string _previousValue = "";
        private string _authoredSnapshot = "";

        //==================== OUTPUTS =====================
        private Action<string> _onChanged;

        /// <summary>Raised when Value changes. Subscribe in OnEnable, unsubscribe in OnDisable.</summary>
        public event Action<string> OnChanged
        {
            add
            {
                _onChanged += value;
#if UNITY_EDITOR
                if (value.Target is Object o && !_editorListeners.Contains(o))
                    _editorListeners.Add(o);
#endif
            }
            remove
            {
                _onChanged -= value;
#if UNITY_EDITOR
                if (value.Target is Object o) _editorListeners.Remove(o);
#endif
            }
        }

        //==================== PUBLIC API =====================
        public string Value
        {
            get => runtimeValue;
            set
            {
                if (string.Equals(runtimeValue, value)) return;
                _previousValue = runtimeValue;
                runtimeValue = value;
                Notify();
            }
        }

        public string InitialValue => initialValue;
        public string PreviousValue => _previousValue;
        public bool IsEmpty => string.IsNullOrEmpty(runtimeValue);
        public int Length => runtimeValue?.Length ?? 0;

        public void SetValue(string v) => Value = v;
        public void Clear() => Value = "";
        public void Append(string suffix) => Value = (runtimeValue ?? "") + suffix;

        /// <summary>Set runtime back to the authored initial value. Fires OnChanged.</summary>
        public void ResetValue() => Value = initialValue;

        /// <summary>Writes the value but does not invoke OnChanged. For restoring saved/default state silently.</summary>
        public void SetValueWithoutNotify(string v)
        {
            _previousValue = runtimeValue;
            runtimeValue = v;
        }

        //==================== UNITY LIFECYCLE =====================
        private void Awake()
        {
            hideFlags = HideFlags.DontUnloadUnusedAsset;
        }

        private void OnEnable()
        {
            _onChanged = null;
#if UNITY_EDITOR
            _editorListeners.Clear();
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#else
            EnterPlay();
#endif
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
#endif
        }

        private void EnterPlay()
        {
            if (!resetOnPlay) return;
            _authoredSnapshot = initialValue;
            runtimeValue = initialValue;
            _previousValue = initialValue;
        }

        private void Notify()
        {
            if (debugLog)
                Debug.Log($"<color=#f75369>[Variable]</color> {name} = \"{runtimeValue}\"", this);
            _onChanged?.Invoke(runtimeValue);
        }

        //==================== EDITOR =====================
#if UNITY_EDITOR
        private readonly List<Object> _editorListeners = new();

        public IReadOnlyList<Object> EditorListeners => _editorListeners;

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                EnterPlay();
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                if (resetOnPlay)
                    initialValue = _authoredSnapshot;
            }
        }

        private void OnValidate()
        {
            if (!Application.isPlaying)
                _onChanged?.Invoke(initialValue);
        }
#endif
    }
}
