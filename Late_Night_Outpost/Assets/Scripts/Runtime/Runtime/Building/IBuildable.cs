// ============================================================================
// IBuildable — contract
//
// A build site that can materialize a building. Mirrors IHarvestable but for
// the opposite verb: instead of yielding a resource, it spends one (later)
// and spawns a prefab.
//
// Separate from IHarvestable on purpose. Harvest and build are different
// verbs with different data and different consequences — Interface
// Segregation. PlayerInteractor consults both contracts independently.
// ============================================================================

namespace Ludocore
{
    /// <summary>Contract for a build site — holds building data and can be built.</summary>
    public interface IBuildable
    {
        BuildingData Data { get; }
        bool CanBuild { get; }
        void Build();
    }
}
