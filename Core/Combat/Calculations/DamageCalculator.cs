using Godot;
using RotOfTime.Core.Combat.Attacks;
using RotOfTime.Core.Combat.Results;
using RotOfTime.Core.Entities;

namespace RotOfTime.Core.Combat.Calculations;

/// <summary>
///     Cálculo de daño centralizado. Sin defensa — la defensa es esquivar.
/// </summary>
public static class DamageCalculator
{
    /// <summary>
    ///     Calcula el daño del ataque aplicando el multiplicador.
    ///     El AttackResult resultante tiene el daño clampado a múltiplo de 5.
    /// </summary>
    public static AttackResult CalculateAttack(EntityStats attacker, AttackData data, float damageMultiplier = 1.0f)
    {
        int raw = Mathf.RoundToInt(attacker.AttackStat * data.DamageCoefficient * damageMultiplier);
        return new AttackResult(raw, data.Name);
    }
}
