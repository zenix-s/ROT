using Godot;
using RotOfTime.Core.Combat.Attacks;

namespace RotOfTime.Core.Combat.Components.AttackMovementComponents;

/// <summary>
///     Abstract base class for projectile movement behaviors.
///     Owns the _PhysicsProcess loop and moves the parent Node2D directly.
///     Subclass this to implement different movement patterns (linear, zigzag, homing, etc.)
/// </summary>
public abstract partial class AttackMovementComponent : Node
{
    protected float Acceleration;
    protected float CurrentSpeed;
    protected Vector2 Direction;
    protected float InitialSpeed;
    protected float TargetSpeed;

    private Node2D _parent;
    private bool _active;

    /// <summary>
    ///     Initialize movement parameters from projectile data and set direction.
    /// </summary>
    public virtual void Initialize(ProjectileData projectileData, Vector2 direction)
    {
        if (projectileData == null) return;
        InitialSpeed = projectileData.InitialSpeed;
        TargetSpeed = projectileData.TargetSpeed;
        Acceleration = (float)projectileData.Acceleration;
        CurrentSpeed = InitialSpeed;
        Direction = direction.Normalized();

        _parent = GetParent<Node2D>();
        _active = true;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_active || _parent == null) return;
        var velocity = CalculateVelocity(Direction, delta);
        _parent.GlobalPosition += velocity * (float)delta;
    }

    /// <summary>
    ///     Calculate the velocity for this frame based on movement pattern.
    /// </summary>
    public abstract Vector2 CalculateVelocity(Vector2 direction, double delta);
}
