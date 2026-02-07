using Godot;
using RotOfTime.Core.Entities.StateMachine;

namespace RotOfTime.Scenes.Enemies.BasicEnemy.StateMachine.States;

public partial class IdleState : State<BasicEnemy>
{
    public override void Enter()
    {
        TargetEntity.Velocity = Vector2.Zero;
    }

    public override void PhysicsProcess(double delta)
    {
        TargetEntity.Velocity = Vector2.Zero;
        TargetEntity.MoveAndSlide();

        if (TargetEntity.Target != null)
        {
            StateMachine.ChangeState<ChasingState>();
        }
    }
}
