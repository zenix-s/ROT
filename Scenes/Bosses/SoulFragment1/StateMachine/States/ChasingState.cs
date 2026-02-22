using Godot;
using RotOfTime.Core.Entities.StateMachine;

namespace RotOfTime.Scenes.Bosses.SoulFragment1.StateMachine.States;

public partial class ChasingState : State<SoulFragment1>
{
    private bool _dashPending;
    private bool _shootPending;

    public override void Enter()
    {
        _dashPending = false;
        _shootPending = false;

        if (TargetEntity.DashTimer != null)
            TargetEntity.DashTimer.Timeout += OnDashTimerFired;
        if (TargetEntity.ShootTimer != null)
            TargetEntity.ShootTimer.Timeout += OnShootTimerFired;
    }

    public override void Exit()
    {
        if (TargetEntity.DashTimer != null)
            TargetEntity.DashTimer.Timeout -= OnDashTimerFired;
        if (TargetEntity.ShootTimer != null)
            TargetEntity.ShootTimer.Timeout -= OnShootTimerFired;
    }

    public override void PhysicsProcess(double delta)
    {
        if (TargetEntity.Target == null)
        {
            StateMachine.ChangeState<IdleState>();
            return;
        }

        // El dash tiene prioridad sobre el disparo
        if (_dashPending)
        {
            _dashPending = false;
            StateMachine.ChangeState<DashChargingState>();
            return;
        }

        if (_shootPending)
        {
            _shootPending = false;
            StateMachine.ChangeState<ShootingState>();
            return;
        }

        var direction = (TargetEntity.Target.GlobalPosition - TargetEntity.GlobalPosition).Normalized();
        TargetEntity.Velocity = direction * TargetEntity.CurrentSpeed;
        TargetEntity.MoveAndSlide();
    }

    private void OnDashTimerFired() => _dashPending = true;
    private void OnShootTimerFired() => _shootPending = true;
}
