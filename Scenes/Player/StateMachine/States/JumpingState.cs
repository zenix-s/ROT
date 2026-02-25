using Godot;
using RotOfTime.Core.Entities.StateMachine;

namespace RotOfTime.Scenes.Player.StateMachine.States;

public partial class JumpingState : State<Player>
{
    public override void Enter()
    {
        TargetEntity.EntityMovementComponent.Jump();
    }

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

        // Apex del salto: velocidad Y se vuelve positiva → caída
        if (TargetEntity.Velocity.Y >= 0f)
            StateMachine.ChangeState<FallingState>();
    }
}
