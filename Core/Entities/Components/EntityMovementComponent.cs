using Godot;

namespace RotOfTime.Core.Entities.Components;

[GlobalClass]
public partial class EntityMovementComponent : Node
{
    public Vector2 Velocity { get; set; } = Vector2.Zero;

    public void StopMovement()
    {
        Velocity = Vector2.Zero;
    }

    public void KnockBack(Vector2 direction, float force)
    {
        Velocity += direction.Normalized() * force;
    }

    public void Move(Vector2 direction, float speed)
    {
        Velocity = direction.Normalized() * speed;
    }
    
    public void Dash(Vector2 direction, float dashSpeed)
    {
        Velocity = direction.Normalized() * dashSpeed;
    }
}
