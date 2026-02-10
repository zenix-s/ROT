using Godot;
using RotOfTime.Core.Combat.Components;
using RotOfTime.Core.Entities.StateMachine;
using RotOfTime.Scenes.Player.Components;

namespace RotOfTime.Scenes.Player.StateMachine.States;

public partial class CastingState : State<Player>
{
    private PlayerAttackSlot? _activeSlot;
    private bool _allowMovement;

    public override void Enter()
    {
        _activeSlot = TargetEntity.ActiveAttackSlot;
        if (_activeSlot == null)
        {
            StateMachine.ChangeState<IdleState>();
            return;
        }

        var spawner = TargetEntity.AttackManager.GetSpawner(_activeSlot.Value);
        _allowMovement = spawner?.AllowMovementDuringCast ?? false;

        TargetEntity.AttackManager.CastCompleted += OnCastCompleted;

        if (!_allowMovement)
        {
            TargetEntity.EntityMovementComponent.StopMovement();
            TargetEntity.Velocity = Vector2.Zero;
        }
    }

    public override void Exit()
    {
        TargetEntity.AttackManager.CastCompleted -= OnCastCompleted;
        TargetEntity.ActiveAttackSlot = null;
        _activeSlot = null;
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

    private void OnCastCompleted(StringName slotName)
    {
        if (!_activeSlot.HasValue)
            return;

        // Compare the signal's StringName with our active slot
        if (slotName != _activeSlot.Value.ToString())
            return;

        Vector2 direction = TargetEntity.EntityInputComponent.Direction;
        if (direction != Vector2.Zero)
            StateMachine.ChangeState<MoveState>();
        else
            StateMachine.ChangeState<IdleState>();
    }
}
