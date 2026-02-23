using Godot;
using RotOfTime.Core.Entities.StateMachine;

namespace RotOfTime.Scenes.Player.StateMachine.States;

public partial class FallingState : State<Player>
{
    public override void PhysicsProcess(double delta)
    {
        var input = TargetEntity.EntityInputComponent;
        var movement = TargetEntity.EntityMovementComponent;

        movement.ApplyGravity(delta);
        movement.Move(input.Direction, Player.Speed);

        TargetEntity.Velocity = movement.Velocity;
        TargetEntity.MoveAndSlide();
        movement.Velocity = TargetEntity.Velocity;

        TargetEntity.TryFireAttack();
        TargetEntity.UpdateFacing(TargetEntity.Velocity.X);

        if (TargetEntity.IsOnFloor())
        {
            if (input.Direction != Vector2.Zero)
                StateMachine.ChangeState<MoveState>();
            else
                StateMachine.ChangeState<IdleState>();
        }
    }
}
