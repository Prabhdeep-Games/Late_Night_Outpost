using UnityEngine;

namespace Ludocore
{
    /// <summary>
    /// Finds the highest-priority tagged target in a Sensor, falling back to a
    /// designated Core transform (looked up by tag at Awake) when nothing closer
    /// is in range. Priority is determined by the order of priorityTags — the
    /// first tag with a candidate in range wins.
    ///
    /// Used by enemies that should opportunistically attack high-value targets
    /// while marching toward a base/hub.
    /// </summary>
    public class PriorityTargeting : Targeting
    {
        //==================== CONFIG =====================
        [Header("Config")]
        [Tooltip("Sensor that detects candidate targets.")]
        [SerializeField] private Sensor sensor;

        [Tooltip("Priority order, top = highest. The sensor must also be filtered to detect these tags.")]
        [SerializeField] private string[] priorityTags = { "Player", "Worker", "Building" };

        [Tooltip("Tag of the fallback target — found by tag at Awake, used when nothing in priorityTags is in range.")]
        [SerializeField] private string coreTag = "Core";

        //==================== STATE =====================
        private Transform _target;
        private Transform _core;
        private float _distance = float.MaxValue;

        public override bool      HasTarget => _target != null;
        public override Transform Target    => _target;
        public override float     Distance  => _distance;

        //==================== LIFECYCLE =====================
        private void Awake()
        {
            var hub = GameObject.FindWithTag(coreTag);
            if (hub) _core = hub.transform;
        }

        private void Update() => Scan();

        //==================== PRIVATE =====================
        // Walk the priority list. For each tag, find the nearest signal matching it.
        // First tag with a hit wins. If no priority hits, fall back to the Core.
        private void Scan()
        {
            _target = null;
            _distance = float.MaxValue;

            var signals = sensor.Signals;

            for (int p = 0; p < priorityTags.Length; p++)
            {
                Transform best = null;
                float bestDistance = float.MaxValue;

                for (int i = 0; i < signals.Count; i++)
                {
                    var obj = signals[i].Object;
                    if (!obj) continue;
                    if (!obj.CompareTag(priorityTags[p])) continue;

                    if (signals[i].Distance < bestDistance)
                    {
                        best = obj.transform;
                        bestDistance = signals[i].Distance;
                    }
                }

                if (best)
                {
                    _target = best;
                    _distance = bestDistance;
                    return;
                }
            }

            // No priority target found — march toward Core if we have one.
            if (_core)
            {
                _target = _core;
                _distance = Vector3.Distance(transform.position, _core.position);
            }
        }
    }
}
