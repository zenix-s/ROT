using System;
using RotOfTime.Core.Combat.Attacks;
using RotOfTime.Core.Entities;

namespace RotOfTime.Core.Combat.Calculations;

/// <summary>
///     Damage calculation (defender side).
///     Formula: FinalDamage = max(1, RawDamage - Defense)
/// </summary>
public static class DamageCalculator
{
    private const int MinimumDamage = 1;

    public static DamageResult Calculate(EntityStats defender, AttackResult attackResult)
    {
        int rawDamage = attackResult.RawDamage;

        int damageAfterDefense = rawDamage - defender.Defense;
        int finalDamage = Math.Max(MinimumDamage, damageAfterDefense);
        int damageReduced = rawDamage - finalDamage;

        bool wasBlocked = damageAfterDefense <= 0;
        if (wasBlocked) return DamageResult.Blocked with { RawDamage = rawDamage, DamageReduced = rawDamage };

        return new DamageResult(
            rawDamage,
            finalDamage,
            attackResult.IsCritical,
            false,
            damageReduced
        );
    }
}
