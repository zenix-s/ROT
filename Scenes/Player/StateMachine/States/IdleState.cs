using Godot;
using RotOfTime.Core.Entities.StateMachine;

namespace RotOfTime.Scenes.Player.StateMachine.States;

public partial class IdleState : State<Player>
{
    public override void Enter()
    {
        TargetEntity.EntityMovementComponent.StopMovement();
    }

    public override void PhysicsProcess(double delta)
    {
        var input = TargetEntity.EntityInputComponent;
        var movement = TargetEntity.EntityMovementComponent;

        movement.ApplyGravity(delta);
        TargetEntity.Velocity = movement.Velocity;
        TargetEntity.MoveAndSlide();
        movement.Velocity = TargetEntity.Velocity; // sync post-MoveAndSlide

        if (!TargetEntity.IsOnFloor())
        {
            StateMachine.ChangeState<FallingState>();
            return;
        }

        if (input.IsJumpJustPressed)
        {
            StateMachine.ChangeState<JumpingState>();
            return;
        }

        if (input.IsDashJustPressed && input.Direction != Vector2.Zero
            && TargetEntity.DashSkill?.IsReady == true)
        {
            StateMachine.ChangeState<DashState>();
            return;
        }

        var fireResult = TargetEntity.TryFireAttack();
        if (fireResult == AttackFireResult.FiredInstant)
            return;

        if (input.Direction != Vector2.Zero)
            StateMachine.ChangeState<MoveState>();
    }
}
