using RotOfTime.Core.Combat.Attacks;
using RotOfTime.Core.Entities;

namespace RotOfTime.Core.Combat.Calculations;

/// <summary>
///     Interface for damage calculation.
///     Allows for different calculation strategies and easy testing.
/// </summary>
public interface IDamageCalculator
{
    /// <summary>
    ///     Calculate damage from attacker to defender using the given attack.
    /// </summary>
    DamageResult Calculate(EntityStats attacker, EntityStats defender, AttackData attack);
}
