// ============================================================================
// IDamageable — Contract
//
// Anything that can take damage and be healed. Implemented by HealthSystem.
// Damage sources (arrows, fire, attackers) call TakeDamage via this interface
// without knowing what kind of entity they're hitting.
// ============================================================================

namespace Ludocore
{
    /// <summary>Anything that can take damage and be healed.</summary>
    public interface IDamageable
    {
        void TakeDamage(float amount);
        void Heal(float amount);
    }
}
