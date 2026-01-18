namespace RotOfTime.Core.Combat.Calculations;

/// <summary>
///     Final damage result after defense calculation.
///     Represents what the defender actually receives.
/// </summary>
public record DamageResult(int RawDamage, int FinalDamage, string AttackName);
