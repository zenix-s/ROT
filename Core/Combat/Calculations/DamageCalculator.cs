using System;
using RotOfTime.Core.Combat.Data;
using RotOfTime.Core.Entities;

namespace RotOfTime.Core.Combat.Calculations;

/// <summary>
///     Centralized damage calculation formulas.
///     All damage computation should go through this class.
/// </summary>
public static class DamageCalculator
{
    private const int MinimumDamage = 1;

    /// <summary>
    ///     Calculate raw damage output from attacker stats and attack data.
    /// </summary>
    public static AttackResult CalculateRawDamage(EntityStats attacker, AttackData data)
    {
        int rawDamage = (int)(attacker.AttackStat * data.DamageCoefficient);
        return new AttackResult(rawDamage, data.Name);
    }

    /// <summary>
    ///     Calculate final damage after applying defender's defense.
    /// </summary>
    public static DamageResult CalculateFinalDamage(AttackResult attack, EntityStats defender)
    {
        int afterDefense = attack.RawDamage - defender.DefenseStat;
        int finalDamage = Math.Max(MinimumDamage, afterDefense);
        return new DamageResult(attack.RawDamage, finalDamage, attack.AttackName);
    }
}
