using Godot;

namespace RotOfTime.Core.Combat.Data;


[GlobalClass]
public partial class AttackData : Resource
{
    [Export] public string Name { get; set; } = "Unnamed Attack";
    [Export] public float DamageCoefficient { get; set; } = 1.0f;

    [ExportCategory("Projectile settings")]
    [Export] public int InitialSpeed { get; set; } = 200;
    [Export] public int TargetSpeed { get; set; } = 200;
    [Export] public double Acceleration { get; set; } = 0;
    [Export] public int Lifetime { get; set; } = 5;
}
