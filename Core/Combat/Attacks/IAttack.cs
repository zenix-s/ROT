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
    void UpdateStats(EntityStats entity, GrimoireStats grimoire);
}
