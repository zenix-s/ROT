using Godot;

namespace RotOfTime.Scenes.Attacks.Projectiles.Fireball;

/// <summary>
///     Fire projectile that inherits base projectile behavior for movement and collision.
///     Spawns an explosion effect on impact or timeout.
/// </summary>
public partial class Fireball : Projectile
{
    [Export] public PackedScene ExplosionEffect { get; set; }

    protected override void OnImpact()
    {
        SpawnExplosion();
        base.OnImpact();
    }

    protected override void BeforeLifetimeTimeout()
    {
        SpawnExplosion();
        base.BeforeLifetimeTimeout();
    }

    private void SpawnExplosion()
    {
        if (ExplosionEffect == null) return;
        var explosion = ExplosionEffect.Instantiate<Node2D>();
        explosion.GlobalPosition = GlobalPosition;
        GetParent()?.AddChild(explosion);
    }
}
