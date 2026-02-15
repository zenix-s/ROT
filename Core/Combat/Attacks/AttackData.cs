using Godot;

namespace RotOfTime.Core.Combat.Attacks;

/// <summary>
///     Complete definition of an attack: damage stats, cooldown, scene reference.
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
}
