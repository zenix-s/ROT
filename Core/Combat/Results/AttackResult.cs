using Godot;

namespace RotOfTime.Core.Combat.Results;

/// <summary>
///     Result of an attack calculation (attacker side).
///     RawDamage is automatically clamped to a multiple of 5 (half-mask unit).
///     Threshold 3: 0-2→0, 3-4→5, 5-7→5, 8-9→10, etc.
/// </summary>
public partial class AttackResult : Resource
{
    public AttackResult() { }

    public AttackResult(int rawDamage, string attackName, bool isCritical = false)
    {
        RawDamage  = ClampToMaskUnit(rawDamage);
        AttackName = attackName;
        IsCritical = isCritical;
    }

    public int    RawDamage  { get; init; }
    public string AttackName { get; init; } = "";
    public bool   IsCritical { get; init; }

    /// <summary>Result for missed or invalid attacks.</summary>
    public static AttackResult None => new(0, "None");

    /// <summary>
    ///     Clampea a múltiplo de 5. Threshold 3: 0-2→0, 3-4→5, 5-7→5, 8-9→10.
    ///     Garantiza que todo daño sea media máscara o máscara completa.
    /// </summary>
    private static int ClampToMaskUnit(int raw)
    {
        if (raw <= 0) return 0;
        int lower = (raw / 5) * 5;
        return (raw % 5) >= 3 ? lower + 5 : lower;
    }
}
