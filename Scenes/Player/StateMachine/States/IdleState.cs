using Godot;
using RotOfTime.Core.Entities.StateMachine;

namespace RotOfTime.Scenes.Player.StateMachine.States;

public partial class IdleState : State<Player>
{
    public override void Enter()
    {
        TargetEntity.EntityMovementComponent.StopMovement();
        TargetEntity.Velocity = Vector2.Zero;
    }

    public override void PhysicsProcess(double delta)
    {
        TargetEntity.Velocity = Vector2.Zero;
        TargetEntity.MoveAndSlide();

        var input = TargetEntity.EntityInputComponent;
        Vector2 direction = input.Direction;

        if (input.IsDashJustPressed && direction != Vector2.Zero)
        {
            StateMachine.ChangeState<DashState>();
            return;
        }

        if (input.IsAttackJustPressed && TargetEntity.ProjectileScene != null)
        {
            StateMachine.ChangeState<AttackState>();
            return;
        }

        if (direction != Vector2.Zero)
        {
            StateMachine.ChangeState<MoveState>();
        }
    }
}