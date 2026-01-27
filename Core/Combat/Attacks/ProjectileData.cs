using Godot;

namespace RotOfTime.Core.Combat.Attacks;

[GlobalClass]
public partial class ProjectileData : AttackData
{
    [ExportCategory("Projectile Settings")]
    [Export]
    public int InitialSpeed { get; set; } = 200;

    [Export] public int TargetSpeed { get; set; } = 200;
    [Export] public double Acceleration { get; set; }
    [Export] public int Lifetime { get; set; } = 5;
}
