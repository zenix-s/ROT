using Godot;
using RotOfTime.Core.Entities.Components;
using RotOfTime.Core.Entities.StateMachine;

namespace RotOfTime.Scenes.Player.StateMachine.States;

public partial class MoveState : State<Player>
{
    public override void PhysicsProcess(double delta)
    {
        EntityInputComponent input = TargetEntity.EntityInputComponent;
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
        PlayerAttackSlot? slot = TargetEntity.EntityInputComponent.GetPressedAttackSlot();
        if (slot == null)
            return false;

        Vector2 mousePos = TargetEntity.GetGlobalMousePosition();
        Vector2 dir = (mousePos - TargetEntity.GlobalPosition).Normalized();
        Vector2 spawnPos = TargetEntity.GlobalPosition + dir * 16;

        bool fired = TargetEntity.AttackManager.TryFire(
            slot: slot.Value, direction: dir, position: spawnPos, stats: TargetEntity.EntityStatsComponent.EntityStats);

        if (!fired)
            return false;

        var spawner = TargetEntity.AttackManager.GetSpawner(slot.Value);
        if (spawner is { IsInstantCast: false })
        {
            TargetEntity.ActiveAttackSlot = slot.Value;
            StateMachine.ChangeState<CastingState>();
        }

        return true;
    }
}
