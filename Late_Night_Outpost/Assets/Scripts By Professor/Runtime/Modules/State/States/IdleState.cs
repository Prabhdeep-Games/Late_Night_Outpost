using UnityEngine;

namespace Ludocore
{
    /// <summary>
    /// The universal "do nothing" state. Always eligible — use it as the absolute
    /// last entry in a StateMachine's priority list, so when every other state has
    /// nothing to do, the agent falls back to standing still.
    ///
    /// No modules, no behavior, no events. The smallest possible state.
    /// </summary>
    public class IdleState : State
    {
        public override bool CanRun() => true;

        // OnEnter, Tick, OnExit intentionally left as the base no-ops.
        // Idle means doing nothing — including doing nothing on entry/exit.
    }
}
