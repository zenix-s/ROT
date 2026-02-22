using Godot;
using RotOfTime.Core.Entities.StateMachine;

namespace RotOfTime.Scenes.Bosses.SoulFragment1.StateMachine.States;

public partial class DashChargingState : State<SoulFragment1>
{
    [Export] public float ChargeDuration { get; set; } = 0.5f;

    private double _elapsed;

    public override void Enter()
    {
        _elapsed = 0;
        TargetEntity.Velocity = Godot.Vector2.Zero;

        // Captura la dirección en este momento (dash directo al punto actual del target)
        if (TargetEntity.Target != null)
        {
            TargetEntity.DashDirection =
                (TargetEntity.Target.GlobalPosition - TargetEntity.GlobalPosition).Normalized();
        }
        else
        {
            TargetEntity.DashDirection = Godot.Vector2.Down;
        }
    }

    public override void PhysicsProcess(double delta)
    {
        _elapsed += delta;

        if (_elapsed >= ChargeDuration)
            StateMachine.ChangeState<DashingState>();
    }
}
