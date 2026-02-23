using Godot;
using RotOfTime.Core.Entities.StateMachine;

namespace RotOfTime.Scenes.Player.StateMachine.States;

public partial class MoveState : State<Player>
{
    public override void PhysicsProcess(double delta)
    {
        var input = TargetEntity.EntityInputComponent;
        var movement = TargetEntity.EntityMovementComponent;

        movement.ApplyGravity(delta);

        if (input.IsDashJustPressed && input.Direction != Vector2.Zero)
        {
            StateMachine.ChangeState<DashState>();
            return;
        }

        if (input.IsJumpJustPressed && TargetEntity.IsOnFloor())
        {
            StateMachine.ChangeState<JumpingState>();
            return;
        }

        if (!TargetEntity.IsOnFloor())
        {
            StateMachine.ChangeState<FallingState>();
            return;
        }

        var fireResult = TargetEntity.TryFireAttack();
        if (fireResult == AttackFireResult.FiredInstant)
            return;

        if (input.Direction == Vector2.Zero)
        {
            StateMachine.ChangeState<IdleState>();
            return;
        }

        movement.Move(input.Direction, Player.Speed);
        TargetEntity.Velocity = movement.Velocity;
        TargetEntity.MoveAndSlide();
        movement.Velocity = TargetEntity.Velocity;
        TargetEntity.UpdateFacing(TargetEntity.Velocity.X);
    }
}
