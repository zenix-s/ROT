using Godot;

namespace RotOfTime.Core.Combat.Results;

/// <summary>
///     Result of an attack calculation (attacker side).
///     Contains the raw attack output before defense is applied.
/// </summary>
public partial class AttackResult : Resource
{
    public AttackResult()
    {
    }

    public AttackResult(int rawDamage, string attackName, bool isCritical = false)
    {
        RawDamage = rawDamage;
        AttackName = attackName;
        IsCritical = isCritical;
    }

    public int RawDamage { get; init; }
    public string AttackName { get; init; } = "";
    public bool IsCritical { get; init; }

    /// <summary>
    ///     Result for missed or invalid attacks.
    /// </summary>
    public static AttackResult None => new(0, "None");
}
