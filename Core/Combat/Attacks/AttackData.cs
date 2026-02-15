using Godot;

namespace RotOfTime.Core.Combat.Attacks;

/// <summary>
///     Complete definition of an attack: damage stats, timing, scene reference.
///     This Resource is the single source of truth for what an attack is.
///     The attack scene itself has no knowledge of its own stats until initialized.
/// </summary>
[GlobalClass]
public partial class AttackData : Resource
{
    [ExportCategory("General Settings")]
    [Export]
    public string Name { get; set; } = "Unnamed Attack";

    [Export] public float DamageCoefficient { get; set; } = 1.0f;

    [ExportCategory("Scene")]
    [Export]
    public PackedScene AttackScene { get; set; }

    [ExportCategory("Timing")]
    [Export]
    public float CooldownDuration { get; set; }

    [Export] public float CastDuration { get; set; }

    [Export] public bool AllowMovementDuringCast { get; set; } = true;

    /// <summary>
    ///     Whether this is an instant cast (CastDuration == 0).
    /// </summary>
    public bool IsInstantCast => CastDuration <= 0f;
}
