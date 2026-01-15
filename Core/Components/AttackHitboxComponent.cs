using Godot;
using RotOfTime.Core.Combat.Attacks;
using RotOfTime.Core.Components;

namespace RotOfTime.Core.Components;

public partial class AttackHitboxComponent : Area2D
{
    [Signal]
    public delegate void AttackConnectedEventHandler();

    public AttackResult AttackResult { get; set; }

    public override void _Ready()
    {
        BodyEntered += OnHitboxBodyEntered;
    }

    public void OnHitboxBodyEntered(Node2D body)
    {
        if (body is not HurtboxComponent) return;

        EmitSignal(SignalName.AttackConnected);
    }

    public override void _ExitTree()
    {
        BodyEntered -= OnHitboxBodyEntered;
    }
}
