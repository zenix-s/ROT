using RotOfTime.Core.Entities;
using RotOfTime.Core.Grimoire;

namespace RotOfTime.Core.Combat.Attacks;

/// <summary>
///     Interface for all attack types.
///     Attacks deal damage and can have hit cooldowns.
/// </summary>
public interface IAttack
{
    AttackResult AttackResult { get; }
    AttackData AttackData { get; }
    AttackTag[] Tags { get; }
    bool CanHit(ulong targetId);
    void RegisterHit(ulong targetId);
    void UpdateStats(EntityStats entity, GrimoireStats grimoire);
}
