using RotOfTime.Core.Combat.Attacks;
using RotOfTime.Core.Entities;
using RotOfTime.Core.Grimoire;

namespace RotOfTime.Core.Combat.Calculations;

/// <summary>
///     Attack calculation (attacker side).
///     Formula: RawDamage = (EntityAttack + GrimoireBonusAttack) * DamageCoefficient
/// </summary>
public static class AttackCalculator
{
    public static AttackResult Calculate(EntityStats entity, GrimoireStats grimoire, AttackData attack)
    {
        int totalAttack = entity.Attack + grimoire.BonusAttack;
        int rawDamage = (int)(totalAttack * attack.DamageCoefficient);

        return new AttackResult(
            rawDamage,
            attack.Name
        );
    }

    /// <summary>
    ///     Base calculation without stats. Uses coefficient as placeholder.
    /// </summary>
    public static AttackResult CalculateBase(AttackData attack)
    {
        return new AttackResult(
            (int)attack.DamageCoefficient,
            attack.Name
        );
    }
}
