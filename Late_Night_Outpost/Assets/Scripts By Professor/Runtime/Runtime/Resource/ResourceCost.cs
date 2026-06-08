// ============================================================================
// ResourceCost — Data layer
//
// Composite of (ResourceData, amount). Used by BuildingData to express
// multi-resource costs like "5 Wood + 2 Stone" as a List<ResourceCost>.
//
// Lives next to ResourceData/ResourceManager so anything that talks about
// resource quantities can reuse it (future: trade, recipes, upkeep…).
// ============================================================================

using System;
using UnityEngine;

namespace Ludocore
{
    /// <summary>A single line item in a resource cost list — which resource, how much.</summary>
    [Serializable]
    public class ResourceCost
    {
        //==================== CONFIG =====================
        [Tooltip("The resource being spent.")]
        [SerializeField] private ResourceData data;

        [Tooltip("How much of the resource is required.")]
        [Min(0)]
        [SerializeField] private int amount = 1;

        public ResourceData Data => data;
        public int Amount => amount;
    }
}
