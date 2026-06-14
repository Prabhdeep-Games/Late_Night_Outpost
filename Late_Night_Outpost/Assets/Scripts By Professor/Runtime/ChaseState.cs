using UnityEngine;

namespace Ludocore
{
    /// <summary>
    /// Drives a NavMeshChase module on while this state is active — the chase
    /// module re-paths toward its own target each frame, so this state has no Tick().
    ///
    /// Trigger:
    ///   Raw       — leave Variable empty: active whenever the chase has a target.
    ///   Gated     — drop in a FloatVariable: also requires its Value to be above
    ///               (or below) Threshold. Use for "chase only while alerted",
    ///               "stop chasing when stamina runs low", etc.
    ///
    /// Place above the calmer fallbacks (Wander, Idle) in the StateMachine's
    /// priority list. Set the NavMeshChase module's autoPlay OFF — this state
    /// owns starting and stopping it.
    /// </summary>
    public class ChaseState : State
    {
        //==================== CONFIG =====================
        [Header("Modules")]
        [Tooltip("The chase module to run while this state is active.")]
        [SerializeField] private NavMeshChase chase;

        [Header("Trigger (Optional)")]
        [Tooltip("Leave empty to chase raw. Assign a variable to gate the chase on its value.")]
        [SerializeField] private FloatVariable variable;

        [Tooltip("Whether the variable must be above or below the threshold for this state to run.")]
        [SerializeField] private Comparison comparison = Comparison.Above;

        [Tooltip("Value the variable is compared against when one is assigned.")]
        [SerializeField] private float threshold = 0.5f;

        //==================== STATE LIFECYCLE =====================
        public override bool CanRun()
        {
            if (!chase.Target) return false;
            if (variable == null) return true;

            return CoreUtils.Compare(variable.Value, comparison, threshold);
        }

        public override void OnEnter() => chase.StartChase();
        public override void OnExit()  => chase.StopChase();
    }
}
