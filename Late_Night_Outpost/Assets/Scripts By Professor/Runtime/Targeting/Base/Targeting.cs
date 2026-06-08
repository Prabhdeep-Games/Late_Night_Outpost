using UnityEngine;

namespace Ludocore
{
    /// <summary>
    /// Base class for "where is my target?" modules. Concrete subclasses
    /// (HarvestableTargeting, PriorityTargeting, …) decide HOW to find a target.
    ///
    /// Anything that needs to act on a target — a controller, a SeekState, an
    /// AttackState — composes a Targeting reference and reads HasTarget /
    /// Target / Distance. The consumer doesn't care how the target was chosen.
    ///
    /// Subclasses typically scan a Sensor each frame and keep the best result
    /// here. They may also expose extra typed accessors (e.g. an IHarvestable).
    /// </summary>
    public abstract class Targeting : MonoBehaviour
    {
        /// <summary>True if a valid target is currently available.</summary>
        public abstract bool HasTarget { get; }

        /// <summary>The current target's Transform. Undefined when HasTarget is false.</summary>
        public abstract Transform Target { get; }

        /// <summary>Distance from this targeting module's GameObject to the current target.</summary>
        public abstract float Distance { get; }
    }
}
