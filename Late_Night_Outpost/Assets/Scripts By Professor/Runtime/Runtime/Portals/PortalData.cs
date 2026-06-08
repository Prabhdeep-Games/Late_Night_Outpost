using UnityEngine;

namespace Ludocore
{
    /// <summary>Configuration for a Portal — defines what it spawns and how often.</summary>
    [CreateAssetMenu(fileName = "NewPortalData", menuName = "Ludocore/Data/Portal Data")]
    public class PortalData : ScriptableObject
    {
        [Header("Spawning")]
        [Tooltip("Seconds between spawns.")]
        [Min(0.1f)]
        public float spawnInterval = 5f;

        [Tooltip("If true, spawn once immediately when the portal activates, then wait spawnInterval before the next one.")]
        public bool firstSpawnImmediate = true;

        [Tooltip("Prefabs this portal can spawn. One is picked at random each interval.")]
        public GameObject[] prefabs;
    }
}
