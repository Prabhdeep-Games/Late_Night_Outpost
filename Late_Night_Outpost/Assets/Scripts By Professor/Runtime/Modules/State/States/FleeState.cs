using UnityEngine;

namespace Ludocore
{
    /// <summary>
    /// Runs away from the current target using a NavMeshFlee module while the
    /// target is dangerously close.
    ///
    /// NavMeshFlee picks a single escape point per call, so this state re-issues
    /// the flee whenever the agent arrives — keeping it moving until the threat
    /// leaves dangerRange or another state takes over.
    ///
    /// Trigger:
    ///   Raw   — leave Variable empty: flee whenever a target is within range.
    ///   Gated — drop in a FloatVariable: also requires its Value below (or above)
    ///           Threshold. Use for "only flee when health is low".
    /// </summary>
    public class FleeState : State
    {
        //==================== CONFIG =====================
        [Header("Modules")]
        [Tooltip("Tells this state where the threat is — flees away from its Target.")]
        [SerializeField] private Targeting targeting;
        [Tooltip("Flee module that picks an escape point away from the threat.")]
        [SerializeField] private NavMeshFlee flee;
        [Tooltip("Motor — used to detect arrival so the flee can be re-issued.")]
        [SerializeField] private NavMeshMotor motor;

        [Header("Behavior")]
        [Tooltip("Flee only while the threat is at or within this distance.")]
        [Min(0f)]
        [SerializeField] private float dangerRange = 6f;

        [Header("Trigger (Optional)")]
        [Tooltip("Leave empty to flee raw. Assign a variable to gate the flee on its value.")]
        [SerializeField] private FloatVariable variable;

        [Tooltip("Whether the variable must be above or below the threshold for this state to run.")]
        [SerializeField] private Comparison comparison = Comparison.Below;

        [Tooltip("Value the variable is compared against when one is assigned.")]
        [SerializeField] private float threshold = 0.3f;

        //==================== STATE LIFECYCLE =====================
        public override bool CanRun()
        {
            if (!targeting.HasTarget || targeting.Distance > dangerRange) return false;
            if (variable == null) return true;

            return CoreUtils.Compare(variable.Value, comparison, threshold);
        }

        public override void OnEnter() => flee.FleeFrom(targeting.Target);

        public override void Tick()
        {
            if (motor.HasArrived) flee.FleeFrom(targeting.Target);
        }

        public override void OnExit() => flee.StopFlee();
    }
}
