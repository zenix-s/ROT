using Godot;
using RotOfTime.Core.Combat.Attacks;

namespace RotOfTime.Core.Components;

/// <summary>
///     Hurtbox component for receiving attacks.
///     Reads AttackResult from any IAttack and signals for damage processing.
/// </summary>
public partial class HurtboxComponent : Area2D
{
    [Signal]
    public delegate void AttackReceivedEventHandler(AttackResult attackResult);

    private Timer _invincibilityTimer;

    public override void _Ready()
    {
        AreaEntered += OnHurtboxAreaEntered;
        _invincibilityTimer = new Timer();
        AddChild(_invincibilityTimer);
        _invincibilityTimer.OneShot = true;
        _invincibilityTimer.Timeout += OnInvincibilityTimeout;
    }

    /// <summary>
    ///     Called by attacks to trigger damage.
    /// </summary>
    public void ReceiveAttack(IAttack attack)
    {
        EmitSignal(SignalName.AttackReceived, attack.AttackResult);
    }

    private void OnHurtboxAreaEntered(Area2D area)
    {
        if (area is not AttackHitboxComponent attack) return;

        // TODO: Implement knockback
        // Vector2 attackerPosition = attack.GetGlobalPosition();
        // Vector2 hurtboxPosition = GetGlobalPosition();
        // Vector2 knockbackDirection = (hurtboxPosition - attackerPosition).Normalized();

        EmitSignal(SignalName.AttackReceived, attack.AttackResult);
    }

    public void StartInvincibility(float duration)
    {
        _invincibilityTimer.WaitTime = duration;

        SetDeferred(Area2D.PropertyName.Monitoring, false);
        SetDeferred(Area2D.PropertyName.Monitorable, false);

        _invincibilityTimer.Start();
    }

    private void OnInvincibilityTimeout()
    {
        SetDeferred(Area2D.PropertyName.Monitoring, true);
        SetDeferred(Area2D.PropertyName.Monitorable, true);
    }
}
