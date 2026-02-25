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

        float distance = TargetEntity.GlobalPosition.DistanceTo(TargetEntity.Target.GlobalPosition);

        if (distance <= TargetEntity.AttackRange)
        {
            StateMachine.ChangeState<AttackingState>();
            return;
        }

        var velocity = TargetEntity.Velocity;
        velocity.Y += TargetEntity.Gravity * (float)delta;

        float dirX = Mathf.Sign(TargetEntity.Target.GlobalPosition.X - TargetEntity.GlobalPosition.X);
        velocity.X = dirX * TargetEntity.Speed;

        TargetEntity.Velocity = velocity;
        TargetEntity.MoveAndSlide();
    }
}
