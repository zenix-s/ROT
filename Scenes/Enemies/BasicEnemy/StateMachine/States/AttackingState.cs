using Godot;
using RotOfTime.Core.Entities.StateMachine;

namespace RotOfTime.Scenes.Enemies.BasicEnemy.StateMachine.States;

/// <summary>
///     Enemy attacks the player when within attack range.
///     Fires BodyAttack via EnemyAttackManager, keeps moving toward target (slower).
///     Returns to ChasingState if target moves out of range, IdleState if target lost.
/// </summary>
public partial class AttackingState : State<BasicEnemy>
{
    public override void Enter()
    {
        // Slow down while attacking (75% of normal speed)
        TargetEntity.Velocity = Vector2.Zero;
    }

    public override void PhysicsProcess(double delta)
    {
        // Lost target entirely
        if (TargetEntity.Target == null)
        {
            StateMachine.ChangeState<IdleState>();
            return;
        }

        float distance = TargetEntity.GlobalPosition.DistanceTo(TargetEntity.Target.GlobalPosition);

        // Target moved out of attack range — chase again
        if (distance > TargetEntity.AttackRange)
        {
            StateMachine.ChangeState<ChasingState>();
            return;
        }

        // Creep toward target while attacking (slower movement)
        Vector2 direction = (TargetEntity.Target.GlobalPosition - TargetEntity.GlobalPosition).Normalized();
        TargetEntity.Velocity = direction * TargetEntity.Speed * 0.4f;
        TargetEntity.MoveAndSlide();

        // Try to fire body attack
        TargetEntity.TryBodyAttack(direction);
    }
}
