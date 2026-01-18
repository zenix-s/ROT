using Godot;
using RotOfTime.Core.Combat.Attacks;
using RotOfTime.Core.Components;
using RotOfTime.Core.Entities;

namespace RotOfTime.Scenes.Attacks.Projectiles.Fireball;

/// <summary>
///     Projectile attack that moves in a direction and damages on contact.
///     Uses CharacterBody2D for physics-based movement.
/// </summary>
public partial class Fireball : CharacterBody2D, IAttack
{
    private Vector2 _direction;
    [Export] public float Speed { get; set; } = 300f;
    [Export] public float Lifetime { get; set; } = 5f;
    [Export] public AttackDamageComponent DamageComponent { get; set; }

    public void UpdateStats(EntityStats ownerStats)
    {
        DamageComponent?.UpdateStats(ownerStats);
    }

    /// <summary>
    ///     Launch the projectile in the specified direction.
    /// </summary>
    public void Launch(Vector2 direction)
    {
        _direction = direction.Normalized();
        Rotation = _direction.Angle();

        GetTree().CreateTimer(Lifetime).Timeout += QueueFree;
    }

    public override void _PhysicsProcess(double delta)
    {
        Velocity = _direction * Speed;
        KinematicCollision2D collision = MoveAndCollide(Velocity * (float)delta);

        if (collision != null)
            // Destroy on hitting a wall/obstacle
            QueueFree();
    }
}
