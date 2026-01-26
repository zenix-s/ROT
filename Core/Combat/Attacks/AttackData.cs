using Godot;

namespace RotOfTime.Core.Combat.Attacks;


[GlobalClass]
public partial class AttackData : Resource
{
    [Export] public string Name { get; set; } = "Unnamed Attack";
    [Export] public float DamageCoefficient { get; set; } = 1.0f;
}
