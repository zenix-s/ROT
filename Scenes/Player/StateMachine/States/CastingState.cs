using Godot;
using RotOfTime.Core.Combat.Components;
using RotOfTime.Core.Entities.StateMachine;

namespace RotOfTime.Scenes.Player.StateMachine.States;

public partial class CastingState : State<Player>
{
    private StringName _activeAttackKey;
    private bool _allowMovement;

    public override void Enter()
    {
        _activeAttackKey = TargetEntity.ActiveAttackKey;
        if (_activeAttackKey == null)
        {
            StateMachine.ChangeState<IdleState>();
            return;
        }

        var metadata = TargetEntity.AttackManagerComponent.GetAttackMetadata(_activeAttackKey);
        _allowMovement = metadata?.AllowMovementDuringCast ?? false;

        TargetEntity.AttackManagerComponent.CastCompleted += OnCastCompleted;

        if (!_allowMovement)
        {
            TargetEntity.EntityMovementComponent.StopMovement();
            TargetEntity.Velocity = Vector2.Zero;
        }
    }

    public override void Exit()
    {
        TargetEntity.AttackManagerComponent.CastCompleted -= OnCastCompleted;
        TargetEntity.ActiveAttackKey = null;
        _activeAttackKey = null;
    }

    public override void PhysicsProcess(double delta)
    {
        var input = TargetEntity.EntityInputComponent;
        Vector2 direction = input.Direction;

        if (input.IsDashJustPressed && direction != Vector2.Zero)
        {
            StateMachine.ChangeState<DashState>();
            return;
        }

        if (_allowMovement)
        {
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
        }
    }

    private void OnCastCompleted(StringName attackKey)
    {
        if (attackKey != _activeAttackKey)
            return;

        Vector2 direction = TargetEntity.EntityInputComponent.Direction;
        if (direction != Vector2.Zero)
            StateMachine.ChangeState<MoveState>();
        else
            StateMachine.ChangeState<IdleState>();
    }
}
