using Godot;

namespace RotOfTime.Core.Grimoire;

/// <summary>
///     Stats for a Grimoire that combine with EntityStats for attack calculations.
/// </summary>
[GlobalClass]
public partial class GrimoireStats : Resource
{
    [Export] public int BonusAttack { get; set; }
    [Export] public float CastingSpeed { get; set; } = 1.0f;
}
