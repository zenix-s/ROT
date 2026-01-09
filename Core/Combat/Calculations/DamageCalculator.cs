using System;
using RotOfTime.Core.Combat.Attacks;
using RotOfTime.Core.Entities;

namespace RotOfTime.Core.Combat.Calculations;

/// <summary>
///     Default damage calculation implementation.
///     Formula: FinalDamage = max(MinimumDamage, (Attack * Coefficient) - Defense)
/// </summary>
public class DamageCalculator : IDamageCalculator
{
    /// <summary>
    ///     Minimum damage that can be dealt (prevents complete nullification).
    /// </summary>
    public int MinimumDamage { get; init; } = 1;

    /// <summary>
    ///     Singleton instance for simple use cases.
    /// </summary>
    public static DamageCalculator Default { get; } = new();

    public DamageResult Calculate(EntityStats attacker, EntityStats defender, AttackData attack)
    {
        int rawDamage = (int)(attacker.Attack * attack.DamageCoefficient);

        int damageAfterDefense = rawDamage - defender.Defense;
        int finalDamage = Math.Max(MinimumDamage, damageAfterDefense);
        int damageReduced = rawDamage - finalDamage;

        bool wasBlocked = finalDamage <= 0;
        if (wasBlocked) return DamageResult.Blocked with { RawDamage = rawDamage, DamageReduced = rawDamage };

        return new DamageResult(
            rawDamage,
            finalDamage,
            false,
            false,
            damageReduced
        );
    }
}
