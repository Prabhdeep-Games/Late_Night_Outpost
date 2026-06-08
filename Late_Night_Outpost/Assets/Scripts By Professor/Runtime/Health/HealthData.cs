// ============================================================================
// HealthData — Data layer
//
// Configuration for HealthSystem. One asset per entity archetype (wolf,
// longhouse, player, etc.). MaxHealth and the optional damage cooldown.
// ============================================================================

using UnityEngine;

namespace Ludocore
{
    [CreateAssetMenu(fileName = "NewHealthData", menuName = "Ludocore/Data/HealthData")]
    public class HealthData : ScriptableObject
    {
        [Header("Stats")]
        [Tooltip("Maximum health points")]
        [Min(0.01f)] public float MaxHealth = 100f;

        [Tooltip("Minimum seconds between consecutive damage hits. 0 = no cooldown.")]
        [Min(0f)] public float DamageCooldown = 0f;
    }
}
