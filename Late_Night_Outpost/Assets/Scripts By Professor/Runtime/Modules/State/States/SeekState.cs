using UnityEngine;

namespace Ludocore
{
    /// <summary>
    /// Moves the agent toward its current target while the target is out of range.
    /// References the abstract Targeting base — so it works with any concrete
    /// targeting module (HarvestableTargeting, PriorityTargeting, ...).
    ///
    /// Active when:  targeting has a target  AND  distance > reachedRange.
    /// Inactive when the target gets close enough — another state (Harvest,
    /// Attack, ...) should take over at that range.
    /// </summary>
    public class SeekState : State
    {
        [Header("Modules")]
        [Tooltip("Tells this state where the target is. Drop in HarvestableTargeting, PriorityTargeting, etc.")]
        [SerializeField] private Targeting targeting;
        [Tooltip("Motor that moves the agent.")]
        [SerializeField] private NavMeshMotor motor;

        [Header("Behavior")]
        [Tooltip("At or below this distance, this state stops being active — something closer-range (Harvest, Attack) should win the cascade.")]
        [Min(0f)]
        [SerializeField] private float reachedRange = 1.5f;

        public override bool CanRun() => targeting.HasTarget && targeting.Distance > reachedRange;

        public override void Tick() => motor.MoveTo(targeting.Target.position);
    }
}
