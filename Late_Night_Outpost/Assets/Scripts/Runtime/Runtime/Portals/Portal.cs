// ============================================================================
// Portal — Logic layer
//
// Spawns prefabs from PortalData at regular intervals via a composed Spawner.
// Portal owns the cadence (its own coroutine); the Spawner owns placement
// (Point/Area/Local/Targeted) and the actual Instantiate.
//
// To gate on day/night or other world state, drive Portal.enabled from
// outside — the coroutine starts on OnEnable, stops on OnDisable.
//
// What gets spawned is the prefab's concern: if it should track itself in
// a registry, it self-registers; if it should die after a time, it carries
// its own Lifecycle. Portal stays type-agnostic.
// ============================================================================

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Ludocore
{
    /// <summary>Spawns prefabs from a PortalData asset at intervals while enabled. Composes a Spawner for placement.</summary>
    public class Portal : MonoBehaviour
    {
        //==================== CONFIG =====================
        [Header("Config")]
        [Tooltip("Data asset defining what to spawn and how often.")]
        [SerializeField] private PortalData data;

        [Tooltip("Spawner used for placement and instantiation. Drag any Spawner subclass (typically AreaSpawner).")]
        [SerializeField] private Spawner spawner;

        [Tooltip("Total spawns this portal will produce before going dormant. 0 = unlimited. PortalSpawnSystem sets this per-night via SetMaxSpawns().")]
        [Min(0)]
        [SerializeField] private int maxSpawns = 0;

        [Header("Registry")]
        [Tooltip("Runtime list of all live portals. This portal registers itself on enable.")]
        [SerializeField] private PortalRegistry registry;

        //==================== STATE =====================
        [Header("Debug")]
        [ReadOnly, SerializeField] private int spawnCount;

        private Coroutine _loop;

        public int SpawnCount => spawnCount;
        public int MaxSpawns => maxSpawns;
        public bool IsExhausted => maxSpawns > 0 && spawnCount >= maxSpawns;

        //==================== OUTPUTS =====================
        public event Action<GameObject> OnSpawned;

        [Header("Events")]
        [Tooltip("Fired each time this portal spawns something. Passes the spawned GameObject.")]
        [SerializeField] private UnityEvent<GameObject> spawnedEvent;

        //==================== LIFECYCLE =====================
        private void OnEnable()
        {
            if (registry) registry.TryAdd(this);
            _loop = StartCoroutine(SpawnLoop());
        }

        private void OnDisable()
        {
            if (registry) registry.Remove(this);
            if (_loop != null) StopCoroutine(_loop);
            _loop = null;
        }

        //==================== INPUTS =====================
        /// <summary>Set how many total spawns this portal will produce. Call right after Instantiate; the spawn loop defers one frame so the value is in place before the first spawn.</summary>
        public void SetMaxSpawns(int n) => maxSpawns = Mathf.Max(0, n);

        /// <summary>Spawn one immediately, picking a random prefab from PortalData.</summary>
        [ContextMenu("Spawn")]
        public void Spawn()
        {
            if (data.prefabs == null || data.prefabs.Length == 0) return;

            var picked = data.prefabs[UnityEngine.Random.Range(0, data.prefabs.Length)];
            if (!picked) return;

            spawner.Spawn(picked);
            spawnCount++;

            var instance = spawner.LastSpawned;
            OnSpawned?.Invoke(instance);
            spawnedEvent?.Invoke(instance);
        }

        //==================== PRIVATE =====================
        private IEnumerator SpawnLoop()
        {
            // Defer one frame so external systems (PortalSpawnSystem) can call
            // SetMaxSpawns before the first spawn fires.
            yield return null;

            if (data.firstSpawnImmediate && CanSpawn()) Spawn();
            while (CanSpawn())
            {
                yield return new WaitForSeconds(data.spawnInterval);
                if (CanSpawn()) Spawn();
            }
            // Quota exhausted — portal goes dormant. The GameObject lives on as
            // a destructible husk (HealthSystem on the same prefab still works).
        }

        private bool CanSpawn() => maxSpawns == 0 || spawnCount < maxSpawns;
    }
}
