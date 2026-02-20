using Godot;
using RotOfTime.Core.Entities.StateMachine;

namespace RotOfTime.Scenes.Enemies.BasicEnemy.StateMachine.States;

/// <summary>
///     Enemy stops and shoots a ranged attack at the player when within attack range.
///     Returns to ChasingState if target moves out of range, IdleState if target lost.
/// </summary>
public partial class AttackingState : State<BasicEnemy>
{
    public override void Enter()
    {
        TargetEntity.Velocity = Vector2.Zero;
    }

    public override void PhysicsProcess(double delta)
    {
        if (TargetEntity.Target == null)
        {
            StateMachine.ChangeState<IdleState>();
            return;
        }

        float distance = TargetEntity.GlobalPosition.DistanceTo(TargetEntity.Target.GlobalPosition);

        if (distance > TargetEntity.AttackRange)
        {
            StateMachine.ChangeState<ChasingState>();
            return;
        }

        // Stand still and shoot
        TargetEntity.Velocity = Vector2.Zero;
        TargetEntity.MoveAndSlide();

        Vector2 direction = (TargetEntity.Target.GlobalPosition - TargetEntity.GlobalPosition).Normalized();
        TargetEntity.TryAttack(direction);
    }
}
