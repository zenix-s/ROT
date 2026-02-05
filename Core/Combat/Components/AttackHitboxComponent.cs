using Godot;
using RotOfTime.Core.Combat.Results;

namespace RotOfTime.Core.Combat.Components;

/// <summary>
///     Hitbox component that detects collisions with hurtboxes.
///     Works with AttackDamageComponent for damage calculation.
/// </summary>
[GlobalClass]
public partial class AttackHitboxComponent : Area2D
{
    [Signal]
    public delegate void AttackConnectedEventHandler();

    public AttackResult AttackResult { get; set; } = AttackResult.None;

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
