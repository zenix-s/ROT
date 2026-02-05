using Godot;
using RotOfTime.Core.Entities.StateMachine;
using RotOfTime.Scenes.Attacks.Projectiles;

namespace RotOfTime.Scenes.Player.StateMachine.States;

public partial class AttackState : State<Player>
{
    private float _cooldownRemaining;

    public override void Enter()
    {
        Vector2 mousePosition = TargetEntity.GetGlobalMousePosition();
        Vector2 directionToMouse = (mousePosition - TargetEntity.GlobalPosition).Normalized();

        Projectile projectile = TargetEntity.ProjectileScene.Instantiate<Projectile>();
        Vector2 spawnOffset = directionToMouse * 16;
        projectile.Execute(
            directionToMouse,
            TargetEntity.GlobalPosition + spawnOffset,
            TargetEntity.EntityStatsComponent.EntityStats);
        TargetEntity.GetParent().AddChild(projectile);

        _cooldownRemaining = TargetEntity.AttackCooldown;
    }

    public override void PhysicsProcess(double delta)
    {
        _cooldownRemaining -= (float)delta;

        var input = TargetEntity.EntityInputComponent;
        Vector2 direction = input.Direction;

        if (input.IsDashJustPressed && direction != Vector2.Zero)
        {
            StateMachine.ChangeState<DashState>();
            return;
        }

        // Allow movement during cooldown
        if (direction != Vector2.Zero)
        {
            TargetEntity.EntityMovementComponent.Move(direction, Player.Speed);
        }
        else
        {
            TargetEntity.EntityMovementComponent.StopMovement();
        }

        TargetEntity.Velocity = TargetEntity.EntityMovementComponent.Velocity;
        TargetEntity.MoveAndSlide();

        if (_cooldownRemaining <= 0)
        {
            if (direction != Vector2.Zero)
                StateMachine.ChangeState<MoveState>();
            else
                StateMachine.ChangeState<IdleState>();
        }
    }
}
