using Godot;

namespace RotOfTime.Core.Combat.Data;

/// <summary>
///     Result of an attack calculation (attacker side).
///     Contains the raw attack output before defense is applied.
/// </summary>
[GlobalClass]
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

    [Export] public int RawDamage { get; set; }
    [Export] public string AttackName { get; set; } = "";
    [Export] public bool IsCritical { get; set; }

    /// <summary>
    ///     Result for missed or invalid attacks.
    /// </summary>
    public static AttackResult None => new(0, "None");
}
