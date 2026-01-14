using Godot;
using RotOfTime.Core.Components.Hurtbox;

namespace RotOfTime.Core.Components;

public partial class AttackHitboxComponent : Area2D
{
    [Signal]
    public delegate void AttackConnectedEventHandler();

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
