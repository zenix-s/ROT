using Godot;
using RotOfTime.Core.Entities.StateMachine;

namespace RotOfTime.Scenes.Bosses.SoulFragment1.StateMachine.States;

public partial class ShootingState : State<SoulFragment1>
{
    private bool _fired;

    public override void Enter()
    {
        _fired = false;
        TargetEntity.Velocity = Godot.Vector2.Zero;
    }

    public override void PhysicsProcess(double delta)
    {
        if (_fired)
        {
            StateMachine.ChangeState<ChasingState>();
            return;
        }

        if (TargetEntity.Target == null)
        {
            StateMachine.ChangeState<IdleState>();
            return;
        }

        TargetEntity.MoveAndSlide();

        var direction = (TargetEntity.Target.GlobalPosition - TargetEntity.GlobalPosition).Normalized();
        Fire(direction);
        _fired = true;
    }

    private void Fire(Vector2 direction)
    {
        if (TargetEntity.IsPhase2)
        {
            // Abanico de 3 proyectiles: -20°, 0°, +20°
            float[] angles = { -20f, 0f, 20f };
            foreach (var angle in angles)
                TargetEntity.TryFire(direction.Rotated(Mathf.DegToRad(angle)));
        }
        else
        {
            TargetEntity.TryFire(direction);
        }
    }
}
