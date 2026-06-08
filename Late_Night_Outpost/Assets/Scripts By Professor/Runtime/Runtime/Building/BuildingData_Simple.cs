// ============================================================================
// BuildingData_Simple — Data layer (L4 version)
//
// A ScriptableObject that defines a building type for the SIMPLE flow: a
// display name and a prefab to spawn. No cost, no resources — the L4 demo
// was entirely pre-economy.
//
// Used by BuildingSite_Simple. The new L5 flow uses BuildingData (no prefab,
// has cost) and BuildingSite with pre-placed pieces in the scene.
// ============================================================================

using UnityEngine;

namespace Ludocore
{
    /// <summary>L4 building definition — name and prefab to spawn.</summary>
    [CreateAssetMenu(fileName = "NewBuildingDataSimple", menuName = "Ludocore/Building Data (Simple)")]
    public class BuildingData_Simple : ScriptableObject
    {
        //==================== CONFIG =====================
        [Header("Config")]
        [Tooltip("Display name shown in the building site prompt.")]
        [SerializeField] private string buildingName;

        [Tooltip("Prefab spawned when this building is built.")]
        [SerializeField] private GameObject prefab;

        public string BuildingName => buildingName;
        public GameObject Prefab => prefab;
    }
}
