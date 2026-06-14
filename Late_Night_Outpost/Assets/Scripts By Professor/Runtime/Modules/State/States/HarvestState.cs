using System;
using UnityEngine;
using UnityEngine.Events;

namespace Ludocore
{
    /// <summary>
    /// Harvests the current target on a cooldown, adding energy to the agent's
    /// Lifecycle. Active while a harvestable target is in range and the agent is
    /// alive.
    ///
    /// References HarvestableTargeting specifically (not the abstract Targeting)
    /// because it needs the typed IHarvestable to call Harvest() on.
    /// </summary>
    public class HarvestState : State
    {
        //==================== CONFIG =====================
        [Header("Modules")]
        [Tooltip("Finds the harvestable target and exposes the typed IHarvestable.")]
        [SerializeField] private HarvestableTargeting targeting;
        [Tooltip("Lifecycle that gains energy from each harvest.")]
        [SerializeField] private Lifecycle lifecycle;
        [Tooltip("Cooldown timer between harvests. Set autoPlay = false, ticks = 1.")]
        [SerializeField] private Timer cooldown;

        [Header("Behavior")]
        [Tooltip("Distance at which the agent can harvest.")]
        [Min(0f)]
        [SerializeField] private float range = 1.5f;
        [Tooltip("Energy gained per successful harvest.")]
        [Min(0f)]
        [SerializeField] private float energyPerHarvest = 20f;

        //==================== OUTPUTS =====================
        public event Action<IHarvestable> OnHarvested;

        [Header("Events")]
        [Tooltip("Fired when a harvest completes (after cooldown). Wire VFX, audio, HUD updates here.")]
        [SerializeField] private UnityEvent harvestedEvent;

        //==================== STATE LIFECYCLE =====================
        public override bool CanRun() =>
            lifecycle.IsAlive
            && targeting.HasTarget
            && targeting.Distance <= range;

        public override void Tick()
        {
            if (cooldown.IsRunning) return;                // wait through cooldown
            if (targeting.Harvestable == null) return;     // target lost mid-frame

            targeting.Harvestable.Harvest();
            lifecycle.AddEnergy(energyPerHarvest);
            cooldown.Restart();

            OnHarvested?.Invoke(targeting.Harvestable);
            harvestedEvent?.Invoke();
        }
    }
}
