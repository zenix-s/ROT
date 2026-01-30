using Godot;
using RotOfTime.Core.Combat.Attacks;

namespace RotOfTime.Core.Combat.Components.AttackMovementComponents;

/// <summary>
///     Abstract base class for projectile movement behaviors.
///     Subclass this to implement different movement patterns (linear, zigzag, homing, etc.)
/// </summary>
public abstract partial class AttackMovementComponent : Node
{
    protected float Acceleration;
    protected float CurrentSpeed;
    protected float InitialSpeed;
    protected float TargetSpeed;

    /// <summary>
    ///     Initialize movement parameters from projectile data.
    /// </summary>
    public virtual void Initialize(ProjectileData projectileData)
    {
        if (projectileData == null) return;
        InitialSpeed = projectileData.InitialSpeed;
        TargetSpeed = projectileData.TargetSpeed;
        Acceleration = (float)projectileData.Acceleration;
        CurrentSpeed = InitialSpeed;
    }

    /// <summary>
    ///     Calculate the velocity for this frame based on movement pattern.
    /// </summary>
    public abstract Vector2 CalculateVelocity(Vector2 currentVelocity, Vector2 direction, double delta);
}
