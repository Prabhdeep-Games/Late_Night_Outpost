using UnityEngine;

namespace Ludocore
{
    /// <summary>
    /// Sends the agent to investigate its current target via a NavMeshInvestigate
    /// module: walk to the point, look around, then return home.
    ///
    /// The investigate module runs its own coroutine once started, so this state
    /// has no Tick() — it just kicks it off on enter and halts it on exit.
    ///
    /// Trigger:
    ///   Raw   — leave Variable empty: investigate whenever a target appears.
    ///   Gated — drop in a FloatVariable: also requires its Value above (or below)
    ///           Threshold. Use for "only investigate once suspicion is high".
    /// </summary>
    public class InvestigateState : State
    {
        //==================== CONFIG =====================
        [Header("Modules")]
        [Tooltip("Tells this state where to investigate — walks to its Target.")]
        [SerializeField] private Targeting targeting;
        [Tooltip("Investigate module: walks to the point, looks around, returns home.")]
        [SerializeField] private NavMeshInvestigate investigate;

        [Header("Trigger (Optional)")]
        [Tooltip("Leave empty to investigate raw. Assign a variable to gate it on its value.")]
        [SerializeField] private FloatVariable variable;

        [Tooltip("Whether the variable must be above or below the threshold for this state to run.")]
        [SerializeField] private Comparison comparison = Comparison.Above;

        [Tooltip("Value the variable is compared against when one is assigned.")]
        [SerializeField] private float threshold = 0.5f;

        //==================== STATE LIFECYCLE =====================
        public override bool CanRun()
        {
            if (!targeting.HasTarget) return false;
            if (variable == null) return true;

            return CoreUtils.Compare(variable.Value, comparison, threshold);
        }

        public override void OnEnter() => investigate.Investigate(targeting.Target);
        public override void OnExit()  => investigate.StopInvestigate();
    }
}
