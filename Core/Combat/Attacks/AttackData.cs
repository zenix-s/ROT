using Godot;

namespace RotOfTime.Core.Combat.Attacks;

/// <summary>
///     Immutable data describing an attack.
///     Godot resources will use instances of this for organizing attacks.
/// </summary>
/// <param name="Name">Attack name (required)</param>
/// <param name="DamageCoefficient">Multiplier applied to attacker's Attack stat (1.0 = 100%)</param>
[GlobalClass]
public partial class AttackData : Resource
{
    [Export] public string Name { get; set; } = "Unnamed Attack";
    [Export] public float DamageCoefficient { get; set; } = 1.0f;
}
