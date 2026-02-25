using Godot;

namespace RotOfTime.Core.Entities.Components;

[GlobalClass]
public partial class EntityMovementComponent : Node
{
    [Export] public float Gravity = 980f;
    [Export] public float JumpVelocity = -420f;

    public Vector2 Velocity { get; set; } = Vector2.Zero;

    public void StopMovement()
    {
        Velocity = new Vector2(0f, Velocity.Y); // preservar Y (gravedad)
    }

    public void KnockBack(Vector2 direction, float force)
    {
        Velocity += direction.Normalized() * force;
    }

    public void Move(Vector2 direction, float speed)
    {
        Velocity = new Vector2(direction.X * speed, Velocity.Y); // solo X, preservar Y
    }

    public void Dash(Vector2 direction, float dashSpeed)
    {
        Velocity = new Vector2(direction.X * dashSpeed, 0f); // dash horizontal, sin gravedad
    }

    public void ApplyGravity(double delta)
    {
        Velocity = new Vector2(Velocity.X, Velocity.Y + Gravity * (float)delta);
    }

    public void Jump()
    {
        Velocity = new Vector2(Velocity.X, JumpVelocity);
    }
}
