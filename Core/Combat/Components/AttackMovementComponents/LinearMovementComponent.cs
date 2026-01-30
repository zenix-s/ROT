using Godot;

namespace RotOfTime.Core.Combat.Components.AttackMovementComponents;

/// <summary>
///     Linear movement component for projectiles.
///     Moves in a straight line, accelerating from initial to target speed.
/// </summary>
public partial class LinearMovementComponent : AttackMovementComponent
{
    public override Vector2 CalculateVelocity(Vector2 currentVelocity, Vector2 direction, double delta)
    {
        CurrentSpeed = Mathf.MoveToward(CurrentSpeed, TargetSpeed, (float)(Acceleration * delta));
        return direction * CurrentSpeed;
    }
}
