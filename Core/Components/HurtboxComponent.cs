using Godot;
using RotOfTime.Core.Combat.Data;
using RotOfTime.Core.Entities;

namespace RotOfTime.Core.Components;

/// <summary>
///     Hurtbox component for receiving attacks.
///     Reads AttackResult from AttackHitboxComponent and signals for damage processing.
///     Relies on i-frames to prevent spam damage.
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
        EmitSignal(SignalName.AttackReceived, attack.DamageComponent?.CurrentAttackResult ?? AttackResult.None);
    }

    private void OnHurtboxAreaEntered(Area2D area)
    {
        if (area is not AttackHitboxComponent hitbox) return;
        EmitSignal(SignalName.AttackReceived, hitbox.AttackResult);
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
