using Godot;
using RotOfTime.Core.Combat.Attacks;

public partial class AttackHitboxComponent : Area2D
{
    [Signal]
    public delegate void AttackConnotedEventHandler(AttackData attackData);

    [Export] public AttackData AttackData;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Monitoring = false; // Attack does not detect collisions
        Monitorable = true; // Hurtboxes can detect this hitbox
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    private void OnAttackHitboxAreaEntered(Area2D area)
    {
        if (area is not HurtboxComponent) return;
        EmitSignal(SignalName.AttackConnoted, AttackData);
    }
}
