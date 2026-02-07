using Godot;
using RotOfTime.Core.Entities.StateMachine;

namespace RotOfTime.Scenes.Enemies.BasicEnemy.StateMachine.States;

public partial class ChasingState : State<BasicEnemy>
{
    public override void PhysicsProcess(double delta)
    {
        if (TargetEntity.Target == null)
        {
            StateMachine.ChangeState<IdleState>();
            return;
        }

        Vector2 direction = (TargetEntity.Target.GlobalPosition - TargetEntity.GlobalPosition).Normalized();
        TargetEntity.Velocity = direction * TargetEntity.Speed;
        TargetEntity.MoveAndSlide();
    }
}
