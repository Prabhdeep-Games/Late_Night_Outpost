// ============================================================================
// PortalRegistry — Runtime registry
//
// Concrete RuntimeSet<Portal> asset. Every live Portal registers itself here on
// enable and removes itself on disable. Other systems (HUD wave counter, "all
// portals destroyed" win checks, minimap markers) read the list or subscribe
// to OnItemAdded / OnItemRemoved without FindObjectsOfType or static lists.
// ============================================================================

using UnityEngine;

namespace Ludocore
{
    [CreateAssetMenu(menuName = "Ludocore/Registries/Portal Registry", fileName = "PortalRegistry")]
    public class PortalRegistry : RuntimeSet<Portal> { }
}
