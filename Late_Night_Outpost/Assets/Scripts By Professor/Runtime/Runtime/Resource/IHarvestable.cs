// ============================================================================
// IHarvestable — contract
//
// The verb-data contract for anything that can be harvested for a resource.
// Exposes WHAT is yielded and an instant Harvest() action used by NPCs and
// scripts that don't need a held timer (e.g. WorkerController).
//
// The player's HOLD-to-harvest flow lives on the separate IHoldInteractable
// contract — Harvestable implements both. Interface Segregation: callers
// pick the contract that matches their need, no one drags in members they
// don't use.
// ============================================================================

namespace Ludocore
{
    /// <summary>Contract for anything that can be harvested for a resource.
    /// Held-input flow is handled separately via IHoldInteractable.</summary>
    public interface IHarvestable
    {
        ResourceData Resource { get; }
        int Yield { get; }
        bool CanHarvest { get; }
        void Harvest();
    }
}
