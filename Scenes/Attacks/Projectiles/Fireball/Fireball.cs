namespace RotOfTime.Scenes.Attacks.Projectiles.Fireball;

/// <summary>
///     Fire projectile that inherits base projectile behavior for movement and collision.
///     Can be extended to add fire-specific effects on timeout or impact.
/// </summary>
public partial class Fireball : Projectile
{
    protected override void BeforeLifetimeTimeout()
    {
        base.BeforeLifetimeTimeout();
    }
}
