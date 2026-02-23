using Godot;
using RotOfTime.Core.Entities.StateMachine;

namespace RotOfTime.Scenes.Player.StateMachine.States;

public partial class DashState : State<Player>
{
    private float _dashTimeRemaining;

    public override void Enter()
    {
        Vector2 direction = TargetEntity.EntityInputComponent.Direction;
        bool executed = TargetEntity.DashSkill.TryExecute(direction, TargetEntity);
        if (!executed)
        {
            StateMachine.ChangeState<IdleState>();
            return;
        }
        TargetEntity.Velocity = TargetEntity.EntityMovementComponent.Velocity;
        _dashTimeRemaining = TargetEntity.DashSkill.Data.Duration;
    }

    public override void PhysicsProcess(double delta)
    {
        _dashTimeRemaining -= (float)delta;

        TargetEntity.Velocity = TargetEntity.EntityMovementComponent.Velocity;
        TargetEntity.MoveAndSlide();

        if (_dashTimeRemaining > 0) return;

        if (!TargetEntity.IsOnFloor())
        {
            StateMachine.ChangeState<FallingState>();
            return;
        }

        Vector2 direction = TargetEntity.EntityInputComponent.Direction;
        if (direction != Vector2.Zero)
            StateMachine.ChangeState<MoveState>();
        else
            StateMachine.ChangeState<IdleState>();
    }
}
