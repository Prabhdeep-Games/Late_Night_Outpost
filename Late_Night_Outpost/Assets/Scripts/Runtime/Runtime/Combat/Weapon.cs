// ============================================================================
// Weapon — Role marker on a wieldable prop
//
// Sits on a prop alongside a Spawner (typically LocalSpawner) and a Grabbable.
// Names that spawner's role: "this is the thing that fires when the wielder
// presses Attack." PlayerAttack queries the held Grabbable for a Weapon and
// calls Fire — separating WHO decides to fire (player, NPC, trap) from WHAT
// happens when fired (the spawner's prefab + offset + cooldown).
//
// The attack prefab itself (referenced by the spawner) carries the actual
// hit logic — DamageZone, lifetime, particle effects, audio, etc. Weapon
// is the tiny bridge between input ownership and spawn mechanics.
// ============================================================================

using UnityEngine;

namespace Ludocore
{
    /// <summary>Marks a prop as wieldable. On Fire, invokes the referenced
    /// Spawner — usually a sibling LocalSpawner pointing at the attack prefab.</summary>
    public class Weapon : MonoBehaviour
    {
        //==================== CONFIG =====================
        [Header("Config")]
        [Tooltip("Spawner to invoke on fire. Typically a sibling LocalSpawner " +
                 "that instantiates the attack prefab in front of the wielder. " +
                 "Cooldown / count / parenting live on the Spawner itself.")]
        [SerializeField] private Spawner spawner;

        //==================== INPUTS =====================
        /// <summary>Fire this weapon. Delegates to the configured spawner — if
        /// it's on cooldown, the spawner silently skips. Safe to call every frame.</summary>
        public void Fire()
        {
            if (spawner) spawner.TrySpawn();
        }
    }
}

// ============================================================================
// Setup on a wieldable prop
//   1. Add a Grabbable (so the player can pick it up via PlayerInteractor).
//   2. Add a Spawner subclass — usually LocalSpawner — and configure:
//      - prefab: the attack to spawn (damage zone, projectile, AOE, etc.)
//      - localOffset: where it appears relative to the prop (e.g. (0,0,0.5))
//      - cooldown: minimum seconds between fires
//   3. Add this Weapon component and drag the Spawner into its field.
//   4. On the player, a PlayerAttack component will call Fire() when the
//      attack input fires AND this prop is the one currently held.
// ============================================================================
