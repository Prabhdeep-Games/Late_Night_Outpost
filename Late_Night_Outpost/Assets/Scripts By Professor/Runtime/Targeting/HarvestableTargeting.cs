using UnityEngine;

namespace Ludocore
{
    /// <summary>
    /// Finds the nearest harvestable in a Sensor that yields a configured ResourceData
    /// and is currently harvestable. Exposes the result through the Targeting API,
    /// plus the typed IHarvestable so consumers (HarvestState, WorkerController) can
    /// call Harvest() on it.
    ///
    /// Same prefab as the agent. Wire the Sensor that detects candidates and the
    /// ResourceData that filters them.
    /// </summary>
    public class HarvestableTargeting : Targeting
    {
        //==================== CONFIG =====================
        [Header("Config")]
        [Tooltip("Sensor that detects candidate harvestables.")]
        [SerializeField] private Sensor sensor;

        [Tooltip("Only harvestables yielding this resource are considered targets.")]
        [SerializeField] private ResourceData targetResource;

        //==================== STATE =====================
        private Transform _target;
        private IHarvestable _harvestable;
        private float _distance = float.MaxValue;

        public override bool      HasTarget => _target != null;
        public override Transform Target    => _target;
        public override float     Distance  => _distance;

        /// <summary>The current target as IHarvestable. Null when HasTarget is false.</summary>
        public IHarvestable Harvestable => _harvestable;

        //==================== LIFECYCLE =====================
        private void Update() => Scan();

        //==================== PRIVATE =====================
        // Walk the sensor's signals, keep the nearest harvestable that yields the
        // configured resource and is currently harvestable. Same logic as the original
        // WorkerController.TryFindTarget — just extracted into its own module so other
        // systems can read the result without re-running the loop.
        private void Scan()
        {
            _target = null;
            _harvestable = null;
            _distance = float.MaxValue;

            var signals = sensor.Signals;
            for (int i = 0; i < signals.Count; i++)
            {
                var obj = signals[i].Object;
                if (!obj) continue;
                if (!obj.TryGetComponent<IHarvestable>(out var h)) continue;
                if (!h.CanHarvest) continue;
                if (h.Resource != targetResource) continue;

                if (signals[i].Distance < _distance)
                {
                    _target = obj.transform;
                    _harvestable = h;
                    _distance = signals[i].Distance;
                }
            }
        }
    }
}
