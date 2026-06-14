using UnityEngine;

namespace Ludocore
{
    /// <summary>
    /// Stops at the target and fires through a Spawner each frame while in range.
    /// The Spawner self-gates with its own cooldown, so the attack rate is
    /// configured on the Spawner, not here.
    ///
    /// References the abstract Targeting base — works with any concrete targeting
    /// module (typically PriorityTargeting for enemies).
    /// </summary>
    public class AttackState : State
    {
        //==================== CONFIG =====================
        [Header("Modules")]
        [Tooltip("Tells this state where the target is.")]
        [SerializeField] private Targeting targeting;
        [Tooltip("Motor — stopped on enter, kept stopped while active.")]
        [SerializeField] private NavMeshMotor motor;
        [Tooltip("Spawner that emits an attack each frame (self-gates via its own cooldown).")]
        [SerializeField] private Spawner attackSpawner;

        [Header("Behavior")]
        [Tooltip("Distance at or under which this state activates.")]
        [Min(0f)]
        [SerializeField] private float range = 4f;

        //==================== STATE LIFECYCLE =====================
        public override bool CanRun() => targeting.HasTarget && targeting.Distance <= range;

        public override void OnEnter() => motor.Stop();

        public override void Tick()
        {
            FaceFlat(targeting.Target.position);
            attackSpawner.Spawn();
        }

        //==================== PRIVATE =====================
        // Flat look — keep the agent upright. Avoids firing angled up or down when
        // the target's pivot is at a different height.
        private void FaceFlat(Vector3 worldPos)
        {
            worldPos.y = transform.position.y;
            if (worldPos != transform.position) transform.LookAt(worldPos);
        }
    }
}
