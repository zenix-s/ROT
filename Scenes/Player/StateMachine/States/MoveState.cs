using Godot;
using RotOfTime.Core.Entities.StateMachine;

namespace RotOfTime.Scenes.Player.StateMachine.States;

public partial class MoveState : State<Player>
{
    public override void PhysicsProcess(double delta)
    {
        var input = TargetEntity.EntityInputComponent;
        Vector2 direction = input.Direction;

        if (input.IsDashJustPressed && direction != Vector2.Zero)
        {
            StateMachine.ChangeState<DashState>();
            return;
        }

        var fireResult = TargetEntity.TryFireAttack();
        if (fireResult == AttackFireResult.FiredNeedsCast)
        {
            StateMachine.ChangeState<CastingState>();
            return;
        }
        if (fireResult == AttackFireResult.FiredInstant)
            return;

        if (direction == Vector2.Zero)
        {
            StateMachine.ChangeState<IdleState>();
            return;
        }

        TargetEntity.EntityMovementComponent.Move(direction, Player.Speed);
        TargetEntity.Velocity = TargetEntity.EntityMovementComponent.Velocity;
        TargetEntity.MoveAndSlide();
    }
}
