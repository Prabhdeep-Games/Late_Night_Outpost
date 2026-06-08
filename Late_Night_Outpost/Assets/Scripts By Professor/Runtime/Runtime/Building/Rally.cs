// ============================================================================
// Rally — Empty marker component
//
// Sits on a child GameObject of a Building to mark the rally point — the spot
// workers walk to on spawn and at dawn. No behavior, no fields. It's a
// tag-by-component, found via Building.Rally (GetComponentInChildren).
// ============================================================================

using UnityEngine;

namespace Ludocore
{
    /// <summary>Empty marker. Place on a child of a Building at the spot workers should gather.</summary>
    public class Rally : MonoBehaviour { }
}
