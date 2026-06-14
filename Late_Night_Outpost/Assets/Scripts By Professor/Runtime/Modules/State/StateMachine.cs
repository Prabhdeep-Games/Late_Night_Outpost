using System;
using UnityEngine;
using UnityEngine.Events;

namespace Ludocore
{
    /// <summary>
    /// Drives a priority-ordered list of State components.
    ///
    /// Each frame:
    ///   1. Walk the States list top to bottom.
    ///   2. The first State whose CanRun() returns true (and is enabled)
    ///      becomes the active state.
    ///   3. If the active state changed since last frame, fire OnExit() on
    ///      the old one and OnEnter() on the new one.
    ///   4. Call Tick() on the active state.
    ///
    /// Wire the State components in priority order — highest priority on top,
    /// fallback (e.g. an always-eligible WanderState) on the bottom.
    /// </summary>
    public class StateMachine : MonoBehaviour
    {
        //==================== CONFIG =====================
        [Header("States")]
        [Tooltip("Checked top to bottom each frame. The first State whose CanRun() returns true becomes active. Drag State components in here in priority order.")]
        [SerializeField] private State[] states;

        //==================== STATE =====================
        [Header("Debug")]
        [Tooltip("Read-only: name of the state currently running.")]
        [ReadOnly, SerializeField] private string currentStateName = "—";

        private State _current;
        private State _previous;

        public State Current  => _current;
        public State Previous => _previous;

        //==================== OUTPUTS =====================
        /// <summary>Fired when the active state changes. Arguments are (from, to) — either may be null.</summary>
        public event Action<State, State> OnStateChanged;

        [Header("Events")]
        [Tooltip("Fired any time the active state changes. Wire VFX, audio, or animator triggers here.")]
        [SerializeField] private UnityEvent stateChangedEvent;

        //==================== LIFECYCLE =====================
        private void Update()
        {
            State next = PickState();
            if (next != _current) Transition(next);
            _current?.Tick();
        }

        private void OnDisable()
        {
            if (_current != null) Transition(null);
        }

        //==================== INPUTS =====================
        /// <summary>
        /// Force a transition to the given state right now. Fires OnExit() on the current
        /// state and OnEnter() on the new one. Pass null to clear the active state.
        ///
        /// Use this for event-driven entries — e.g. a UnityEvent wired to this method,
        /// a GameEvent listener, or an animation event.
        ///
        /// IMPORTANT: the priority cascade reasserts itself on the next Update. If the
        /// state you change to has CanRun() returning false, a higher-priority eligible
        /// state will replace it next frame. To make a forced state "stick" for a
        /// duration, give it an internal timer started in OnEnter() and have CanRun()
        /// return true while that timer is non-zero.
        /// </summary>
        public void ChangeTo(State next)
        {
            if (next == _current) return;
            Transition(next);
        }

        [ContextMenu("Log Active State")]
        private void LogActiveState()
        {
            string prev = _previous ? _previous.GetType().Name : "—";
            Debug.Log($"[{name}] Current: {currentStateName}    Previous: {prev}", this);
        }

        //==================== PRIVATE =====================
        // Walk the priority list; return the first enabled state whose CanRun() is true.
        // Returns null if nothing matches — the machine then sits idle until something does.
        private State PickState()
        {
            for (int i = 0; i < states.Length; i++)
            {
                if (!states[i].enabled) continue;
                if (states[i].CanRun()) return states[i];
            }
            return null;
        }

        // Fire OnExit on the old state, swap, fire OnEnter on the new, broadcast events.
        // Shared between the per-frame cascade (Update) and the public ChangeTo() entry point.
        private void Transition(State next)
        {
            _current?.OnExit();
            _previous = _current;
            _current = next;
            _current?.OnEnter();

            currentStateName = _current ? _current.GetType().Name : "—";
            OnStateChanged?.Invoke(_previous, _current);
            stateChangedEvent?.Invoke();
        }
    }
}

// ============================================================================
// Setup on a prefab
//   1. Add the StateMachine component to the agent's root GameObject.
//   2. Add one or more State subclass components (WanderState, SeekState,
//      HarvestState, …) to the same GameObject.
//   3. Drag the State components into the StateMachine's States list.
//      Top entries have higher priority. Usually the most "demanding"
//      state goes first (e.g. Harvest), the fallback last (e.g. Wander,
//      whose CanRun() always returns true).
//   4. Untick a State's enabled checkbox to temporarily disable it from the
//      cascade without removing it from the list — useful when testing.
//   5. Watch the StateMachine's "Current State Name" field in Play mode to
//      see which state is active. Right-click → Log Active State to print
//      current + previous to the console.
// ============================================================================
