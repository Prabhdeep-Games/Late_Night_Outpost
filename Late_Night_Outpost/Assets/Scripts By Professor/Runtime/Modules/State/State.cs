using UnityEngine;

namespace Ludocore
{
    /// <summary>
    /// One behaviour an agent can be in (e.g. Wander, Seek, Harvest, Attack).
    /// Drop a State subclass onto the same GameObject as a StateMachine and
    /// wire it into that machine's priority list.
    ///
    /// IMPORTANT: do NOT override Unity's Update() in a State subclass. Per-frame
    /// logic belongs in Tick(), which the StateMachine only calls while this state
    /// is the active one. Update() would run every frame regardless.
    /// </summary>
    public abstract class State : MonoBehaviour
    {
        /// <summary>
        /// Return true when this state should be active given current world state.
        /// The StateMachine checks each state in priority order every frame and
        /// runs the first one that says yes — so CanRun() should be cheap and
        /// stateless. It is called even when this state is not currently active.
        /// </summary>
        public abstract bool CanRun();

        /// <summary>Called once when this state becomes active. Setup, animation cues, etc.</summary>
        public virtual void OnEnter() { }

        /// <summary>Called every frame while this state is active. Per-frame logic goes here.</summary>
        public virtual void Tick() { }

        /// <summary>Called once when leaving this state. Cleanup, stop motors, hide UI, etc.</summary>
        public virtual void OnExit() { }
    }
}

// ============================================================================
// Writing a State
//
//   public class WanderState : State
//   {
//       [SerializeField] private NavMeshWander wander;
//
//       public override bool CanRun()    => true;               // fallback — always eligible
//       public override void OnEnter()   => wander.enabled = true;
//       public override void OnExit()    => wander.enabled = false;
//       // Tick() not needed — the wander module ticks itself once enabled.
//   }
//
//   public class HarvestState : State
//   {
//       [SerializeField] private HarvestableTargeting targeting;
//       [SerializeField] private Lifecycle lifecycle;
//       [SerializeField] private Timer cooldown;
//       [SerializeField, Min(0f)] private float range = 1.5f;
//
//       public override bool CanRun() =>
//           lifecycle.IsAlive
//           && targeting.TryGetBest(out var signal, out _)
//           && signal.Distance <= range;
//
//       public override void Tick()
//       {
//           if (cooldown.IsRunning) return;
//           if (!targeting.TryGetBest(out _, out var target)) return;
//           target.Harvest();
//           cooldown.Restart();
//       }
//   }
//
// Modules a state depends on (Lifecycle, Sensor, Motor, Timer, …) are wired
// with [SerializeField] — exactly like any other Ludocore module.
// ============================================================================
