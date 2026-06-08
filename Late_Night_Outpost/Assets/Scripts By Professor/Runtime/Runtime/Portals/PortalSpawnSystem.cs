// ============================================================================
// PortalSpawnSystem — Glue manager
//
// Decides how the night escalates. On a call to SpawnNightWave() it:
//   1. Reads the current day from a DayCount IntVariable.
//   2. Evaluates two CurveData assets to get
//        K = how many portals tonight
//        N = how many enemies per portal tonight
//   3. Picks K random candidate zones (no repeats) from a list of scene-placed
//      AreaSpawner markers, calls SpawnOne() on each, and sets the resulting
//      Portal's quota to N.
//
// Difficulty lives here and nowhere else. Tonight's numbers come from two
// curve assets a designer can edit without touching code.
//
// WIRING
//   - Put this on a single scene GameObject (it's a manager, not a module).
//   - Drop in the DayCount IntVariable and the two CurveData assets.
//   - Place N AreaSpawner markers in the scene where portals may appear; each
//     marker's prefab field = the portal prefab. Drag them into candidateZones.
//   - Add a GameEventListener on the same GameObject listening to the OnDusk
//     channel and call SpawnNightWave() via its UnityEvent response.
// ============================================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Ludocore
{
    /// <summary>Spawns the portals for a single night, with count and per-portal enemy quota driven by day-count curves.</summary>
    public class PortalSpawnSystem : MonoBehaviour
    {
        //==================== CONFIG =====================
        [Header("Source")]
        [Tooltip("Current day number. Read each night to drive the difficulty curves.")]
        [SerializeField] private IntVariable dayCount;

        [Header("Difficulty Curves")]
        [Tooltip("Curve mapping day -> number of portals to spawn that night.")]
        [SerializeField] private CurveData portalCountCurve;

        [Tooltip("Curve mapping day -> number of enemies each portal will produce that night.")]
        [SerializeField] private CurveData enemiesPerPortalCurve;

        [Header("Placement")]
        [Tooltip("Candidate zones where portals may appear. Each AreaSpawner's prefab field must be the portal prefab. Each night picks a random subset (no repeats).")]
        [SerializeField] private AreaSpawner[] candidateZones;

        //==================== STATE =====================
        [Header("Debug")]
        [ReadOnly, SerializeField] private int lastNightPortalCount;
        [ReadOnly, SerializeField] private int lastNightEnemiesPerPortal;
        [ReadOnly, SerializeField] private int lastNightDay;

        public int LastNightPortalCount => lastNightPortalCount;
        public int LastNightEnemiesPerPortal => lastNightEnemiesPerPortal;

        //==================== OUTPUTS =====================
        [Header("Events")]
        [Tooltip("Fired after a night wave has been spawned, passes (portals, enemiesPerPortal).")]
        [SerializeField] private UnityEvent<int, int> nightWaveSpawnedEvent;

        //==================== INPUTS =====================
        /// <summary>Wire this to the OnDusk GameEvent via a GameEventListener's UnityEvent response.</summary>
        [ContextMenu("Spawn Night Wave")]
        public void SpawnNightWave()
        {
            int day = dayCount.Value;
            int portalsTonight = Mathf.Max(0, Mathf.RoundToInt(portalCountCurve.Evaluate(day)));
            int enemiesEach    = Mathf.Max(0, Mathf.RoundToInt(enemiesPerPortalCurve.Evaluate(day)));

            portalsTonight = Mathf.Min(portalsTonight, candidateZones.Length);

            // Pick portalsTonight zones at random, without repeats, by partial Fisher-Yates.
            _pool.Clear();
            _pool.AddRange(candidateZones);
            for (int i = 0; i < portalsTonight; i++)
            {
                int j = Random.Range(i, _pool.Count);
                (_pool[i], _pool[j]) = (_pool[j], _pool[i]);
            }

            for (int i = 0; i < portalsTonight; i++)
            {
                var zone = _pool[i];
                if (!zone) continue;
                zone.SpawnOne();

                var spawned = zone.LastSpawned;
                if (!spawned) continue;

                var portal = spawned.GetComponent<Portal>();
                if (portal) portal.SetMaxSpawns(enemiesEach);
            }

            lastNightDay = day;
            lastNightPortalCount = portalsTonight;
            lastNightEnemiesPerPortal = enemiesEach;
            nightWaveSpawnedEvent?.Invoke(portalsTonight, enemiesEach);
        }

        //==================== PRIVATE =====================
        private readonly List<AreaSpawner> _pool = new();
    }
}

// ============================================================================
// Scene setup checklist
//   1. Create the two curve assets:
//      Right-click → Create → Ludocore → Data → Curve
//      Name them PortalCountPerNight.asset and EnemiesPerPortal.asset.
//      Open each in the inspector and draw the difficulty ramp you want.
//   2. Create a PortalRegistry asset:
//      Right-click → Create → Ludocore → Registries → Portal Registry.
//      Drag it into the Portal prefab's Registry slot.
//   3. Place AreaSpawner markers in the scene at every candidate portal spot.
//      Set each one's prefab field to your portal prefab. Tune the box size
//      to define the spawn area.
//   4. On a fresh GameObject ("PortalSpawnSystem"), add this component:
//      - Drop in the DayCount IntVariable.
//      - Drop in PortalCountPerNight.asset and EnemiesPerPortal.asset.
//      - Drag every scene AreaSpawner marker into candidateZones.
//   5. On the same GameObject, add a GameEventListener:
//      - GameEvent = your OnDusk channel (from DayNightController).
//      - Response UnityEvent → PortalSpawnSystem.SpawnNightWave().
// ============================================================================
