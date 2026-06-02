// ============================================
// Int Variable
// ============================================
// Reactive shared int. One writer sets Value; many readers observe via OnChanged.
//
// Authored value (initialValue) is what the designer sets in edit mode.
// Runtime value (runtimeValue) tracks live state during play.
// On exiting play, the authored value is restored from a snapshot — designer
// tweaks made during play are reverted (intentional; stop play to author).
//
// Optional clamping: tick IsClamped and set Min/Max for designer-enforced bounds.
// Ratio returns InverseLerp(Min, Max, Value) — useful for HUD bars.
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
    [CreateAssetMenu(fileName = "NewIntVariable", menuName = "Ludocore/Variables/Int")]
    public class IntVariable : ScriptableObject
    {
        //==================== INITIAL VALUE =====================
        [Header("Initial Value")]
        [Tooltip("Authored value the designer sets. Edits during play are reverted on play exit.")]
        [SerializeField] private int initialValue;

        //==================== BOUNDS =====================
        [Header("Bounds (Optional)")]
        [Tooltip("Clamp Value between Min and Max.")]
        [SerializeField] private bool isClamped = false;

        [Tooltip("Lower bound when IsClamped.")]
        [SerializeField] private int min = 0;

        [Tooltip("Upper bound when IsClamped.")]
        [SerializeField] private int max = 100;

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
        [ReadOnly, SerializeField] private int runtimeValue;

        //==================== INTERNAL =====================
        private int _previousValue;
        private int _authoredSnapshot;

        //==================== OUTPUTS =====================
        private Action<int> _onChanged;

        /// <summary>Raised when Value changes. Subscribe in OnEnable, unsubscribe in OnDisable.</summary>
        public event Action<int> OnChanged
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
        public int Value
        {
            get => runtimeValue;
            set
            {
                int clamped = isClamped ? Mathf.Clamp(value, min, max) : value;
                if (runtimeValue == clamped) return;
                _previousValue = runtimeValue;
                runtimeValue = clamped;
                Notify();
            }
        }

        public int InitialValue => initialValue;
        public int PreviousValue => _previousValue;
        public bool IsClamped => isClamped;
        public int Min => min;
        public int Max => max;

        /// <summary>InverseLerp(Min, Max, Value) as a 0..1 float. 0 when not clamped.</summary>
        public float Ratio => isClamped ? Mathf.InverseLerp(min, max, runtimeValue) : 0f;

        public void Add(int delta) => Value = runtimeValue + delta;
        public void Increment() => Value = runtimeValue + 1;
        public void Decrement() => Value = runtimeValue - 1;
        public void SetValue(int v) => Value = v;

        /// <summary>Set runtime back to the authored initial value. Fires OnChanged.</summary>
        public void ResetValue() => Value = initialValue;

        /// <summary>Writes the value but does not invoke OnChanged. For restoring saved/default state silently.</summary>
        public void SetValueWithoutNotify(int v)
        {
            int clamped = isClamped ? Mathf.Clamp(v, min, max) : v;
            _previousValue = runtimeValue;
            runtimeValue = clamped;
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
                Debug.Log($"<color=#f75369>[Variable]</color> {name} = {runtimeValue}", this);
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
            if (isClamped)
            {
                if (max < min) max = min;
                initialValue = Mathf.Clamp(initialValue, min, max);
            }
            if (!Application.isPlaying)
                _onChanged?.Invoke(initialValue);
        }
#endif
    }
}
