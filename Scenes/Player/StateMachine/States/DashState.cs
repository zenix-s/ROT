using Godot;
using RotOfTime.Core.Entities.StateMachine;

namespace RotOfTime.Scenes.Player.StateMachine.States;

public partial class DashState : State<Player>
{
    private const float DashSpeed = 600f;
    private const float DashDuration = 0.2f;

    private float _dashTimeRemaining;

    public override void Enter()
    {
        Vector2 direction = TargetEntity.EntityInputComponent.Direction;
        TargetEntity.EntityMovementComponent.Dash(direction, DashSpeed);
        TargetEntity.Velocity = TargetEntity.EntityMovementComponent.Velocity;
        _dashTimeRemaining = DashDuration;
    }

    public override void PhysicsProcess(double delta)
    {
        _dashTimeRemaining -= (float)delta;

        TargetEntity.Velocity = TargetEntity.EntityMovementComponent.Velocity;
        TargetEntity.MoveAndSlide();

        if (_dashTimeRemaining <= 0)
        {
            Vector2 direction = TargetEntity.EntityInputComponent.Direction;
            if (direction != Vector2.Zero)
                StateMachine.ChangeState<MoveState>();
            else
                StateMachine.ChangeState<IdleState>();
        }
    }
}
