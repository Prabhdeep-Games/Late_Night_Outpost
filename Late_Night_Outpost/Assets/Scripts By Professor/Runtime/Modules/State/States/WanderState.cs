using UnityEngine;

namespace Ludocore
{
    /// <summary>
    /// Wanders by toggling a NavMeshWander module on while this state is active.
    /// Use as a fallback when nothing more interesting is happening — place it
    /// near the bottom of the StateMachine's priority list (just above IdleState
    /// if you include both).
    ///
    /// The wander module ticks itself once enabled — this state has no Tick().
    /// </summary>
    public class WanderState : State
    {
        [Header("Modules")]
        [Tooltip("The wander module to enable while this state is active.")]
        [SerializeField] private NavMeshWander wander;

        public override bool CanRun() => true;

        public override void OnEnter() => wander.enabled = true;
        public override void OnExit()  => wander.enabled = false;
    }
}
