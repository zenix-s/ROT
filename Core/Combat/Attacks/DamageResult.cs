namespace RotOfTime.Core.Combat.Attacks;

/// <summary>
///     Immutable result of a damage calculation.
///     Contains all information needed to apply and display damage.
/// </summary>
/// <param name="RawDamage">Damage before defense reduction</param>
/// <param name="FinalDamage">Damage after all modifiers (what actually gets applied)</param>
/// <param name="WasCritical">Whether this was a critical hit</param>
/// <param name="WasBlocked">Whether damage was fully blocked (0 final damage)</param>
/// <param name="DamageReduced">Amount absorbed by defense</param>
public readonly record struct DamageResult(
    int RawDamage,
    int FinalDamage,
    bool WasCritical = false,
    bool WasBlocked = false,
    int DamageReduced = 0
)
{
    /// <summary>
    ///     Result when attack deals no damage (fully blocked).
    /// </summary>
    public static DamageResult Blocked => new(0, 0, WasBlocked: true);

    /// <summary>
    ///     Result when attack misses or is invalid.
    /// </summary>
    public static DamageResult None => new(0, 0);
}
