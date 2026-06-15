using UnityEngine;

namespace Ludocore
{
    /// <summary>
    /// Walks the agent back to where it started (or a Home transform). Meant as a
    /// low-priority "nothing's going on, return to post" behaviour — place it near
    /// the bottom of the StateMachine's priority list, below Chase/Seek/Attack.
    ///
    /// Trigger:
    ///   Raw   — leave Variable empty: always eligible (the resting fallback).
    ///   Gated — drop in a FloatVariable: only go home while its Value meets the
    ///           comparison. Default AtOrBelow 0 means "head home once awareness
    ///           has fully drained back to 0".
    /// </summary>
    public class ReturnHomeState : State
    {
        //==================== CONFIG =====================
        [Header("Modules")]
        [Tooltip("Motor that moves the agent home.")]
        [SerializeField] private NavMeshMotor motor;

        [Header("Behavior")]
        [Tooltip("Optional home to return to. Leave empty to use the agent's position at scene start.")]
        [SerializeField] private Transform home;

        [Header("Trigger (Optional)")]
        [Tooltip("Leave empty to always be eligible. Assign a variable to gate the return on its value.")]
        [SerializeField] private FloatVariable variable;

        [Tooltip("Whether the variable must be above or below the threshold for this state to run.")]
        [SerializeField] private Comparison comparison = Comparison.AtOrBelow;

        [Tooltip("Value the variable is compared against when one is assigned.")]
        [SerializeField] private float threshold = 0f;

        //==================== STATE =====================
        private Vector3 _homePosition;

        //==================== LIFECYCLE =====================
        private void Awake()
        {
            // Capture the spawn point now — "home" is where the agent started,
            // not wherever it happens to be when this state activates.
            _homePosition = transform.position;
        }

        //==================== STATE LIFECYCLE =====================
        public override bool CanRun()
        {
            if (variable == null) return true;
            return CoreUtils.Compare(variable.Value, comparison, threshold);
        }

        public override void OnEnter()
            => motor.MoveTo(home ? home.position : _homePosition);
    }
}