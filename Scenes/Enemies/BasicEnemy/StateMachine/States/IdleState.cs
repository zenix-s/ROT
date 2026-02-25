using Godot;
using RotOfTime.Core.Entities.StateMachine;

namespace RotOfTime.Scenes.Enemies.BasicEnemy.StateMachine.States;

public partial class IdleState : State<BasicEnemy>
{
    public override void Enter()
    {
        TargetEntity.Velocity = new Vector2(0f, TargetEntity.Velocity.Y);
    }

    public override void PhysicsProcess(double delta)
    {
        var velocity = TargetEntity.Velocity;
        velocity.Y += TargetEntity.Gravity * (float)delta;
        velocity.X = 0f;
        TargetEntity.Velocity = velocity;
        TargetEntity.MoveAndSlide();

        if (TargetEntity.Target != null)
        {
            StateMachine.ChangeState<ChasingState>();
        }
    }
}
