// ============================================================================
// ResourceManager — Scene-wide service
//
// Tracks how much of each resource the scene currently holds. Add(...) from
// any harvester; listeners (HUD, build costs later) subscribe to OnChanged.
//
// Singleton — one instance per scene, accessed via ResourceManager.Instance.
// The singleton pattern was introduced in L4; this is its first real use.
//
// Uses a Dictionary<ResourceData, int>. Previews the Dictionary lesson
// planned for later in the course.
// ============================================================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ludocore
{
    /// <summary>Scene-wide stockpile of resources.</summary>
    public class ResourceManager : MonoBehaviour
    {
        //==================== SINGLETON =====================
        public static ResourceManager Instance { get; private set; }

        //==================== STATE =====================
        private readonly Dictionary<ResourceData, int> _amounts = new();

        //==================== OUTPUTS =====================
        /// <summary>Fired when an amount changes. (resource, newAmount, delta)</summary>
        public event Action<ResourceData, int, int> OnChanged;

        //==================== LIFECYCLE =====================
        private void Awake()
        {
            if (Instance && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        //==================== INPUTS =====================
        /// <summary>Current amount held of a resource (0 if never added).</summary>
        public int Get(ResourceData resource)
        {
            if (!resource) return 0;
            return _amounts.TryGetValue(resource, out int amount) ? amount : 0;
        }

        /// <summary>Add to the stockpile. Fires OnChanged.</summary>
        public void Add(ResourceData resource, int amount)
        {
            if (!resource) return;
            if (amount == 0) return;

            _amounts.TryGetValue(resource, out int current);
            current += amount;
            _amounts[resource] = current;

            OnChanged?.Invoke(resource, current, amount);
        }

        /// <summary>Try to spend. Returns false if insufficient; nothing changes in that case.</summary>
        public bool TryConsume(ResourceData resource, int amount)
        {
            if (!resource) return false;
            if (amount <= 0) return false;

            _amounts.TryGetValue(resource, out int current);
            if (current < amount) return false;

            current -= amount;
            _amounts[resource] = current;

            OnChanged?.Invoke(resource, current, -amount);
            return true;
        }

        /// <summary>True when every cost in the list can be paid from the current stockpile.</summary>
        public bool CanAfford(IReadOnlyList<ResourceCost> costs)
        {
            if (costs == null || costs.Count == 0) return true;
            for (int i = 0; i < costs.Count; i++)
            {
                ResourceCost c = costs[i];
                if (c == null || !c.Data || c.Amount <= 0) continue;
                if (Get(c.Data) < c.Amount) return false;
            }
            return true;
        }

        /// <summary>Atomic multi-resource spend. Verifies the full list first, then commits — partial spends never happen.</summary>
        public bool TryConsume(IReadOnlyList<ResourceCost> costs)
        {
            if (costs == null || costs.Count == 0) return true;
            if (!CanAfford(costs)) return false;

            for (int i = 0; i < costs.Count; i++)
            {
                ResourceCost c = costs[i];
                if (c == null || !c.Data || c.Amount <= 0) continue;
                TryConsume(c.Data, c.Amount);
            }
            return true;
        }
    }
}

// ============================================================================
// Setup in a scene
//   1. Create an empty GameObject named "ResourceManager" (persistent, near
//      the root of the scene hierarchy).
//   2. Add this ResourceManager component.
//   3. That is it — Harvestables find it via ResourceManager.Instance and
//      deposit to it directly. ResourceUI subscribes to OnChanged.
// ============================================================================
