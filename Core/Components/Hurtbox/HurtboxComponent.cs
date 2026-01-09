using Godot;
using RotOfTime.Core.Combat.Attacks;

public partial class HurtboxComponent : Area2D
{
    [Signal]
    public delegate void AttackReceivedEventHandler(AttackData attackData);

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    private void OnHurtboxAreaEntered(Area2D area)
    {
        // Handle hurtbox collision logic here
        if (area is not AttackHitboxComponent hitbox) return;

        EmitSignal(SignalName.AttackReceived, hitbox.AttackData);
    }
}
