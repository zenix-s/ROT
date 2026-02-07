using Godot;
using RotOfTime.Core.Combat;
using RotOfTime.Core.Combat.Components;
using RotOfTime.Core.Entities.StateMachine;

namespace RotOfTime.Scenes.Player.StateMachine.States;

public partial class MoveState : State<Player>
{
    public override void PhysicsProcess(double delta)
    {
        var input = TargetEntity.EntityInputComponent;
        Vector2 direction = input.Direction;

        if (input.IsDashJustPressed && direction != Vector2.Zero)
        {
            StateMachine.ChangeState<DashState>();
            return;
        }

        if (TryFireAttack())
            return;

        if (direction == Vector2.Zero)
        {
            StateMachine.ChangeState<IdleState>();
            return;
        }

        TargetEntity.EntityMovementComponent.Move(direction, Player.Speed);
        TargetEntity.Velocity = TargetEntity.EntityMovementComponent.Velocity;
        TargetEntity.MoveAndSlide();
    }

    private bool TryFireAttack()
    {
        StringName key = TargetEntity.EntityInputComponent.GetPressedAttackKey();
        if (key == null)
            return false;

        Vector2 mousePos = TargetEntity.GetGlobalMousePosition();
        Vector2 dir = (mousePos - TargetEntity.GlobalPosition).Normalized();
        Vector2 spawnPos = TargetEntity.GlobalPosition + dir * 16;

        IAttack attack = TargetEntity.AttackManagerComponent.TryFire(
            key, dir, spawnPos, TargetEntity.EntityStatsComponent.EntityStats);

        if (attack == null)
            return false;

        var metadata = TargetEntity.AttackManagerComponent.GetAttackMetadata(key);
        if (metadata is { IsInstantCast: false })
        {
            TargetEntity.ActiveAttackKey = key;
            StateMachine.ChangeState<CastingState>();
        }

        return true;
    }
}
