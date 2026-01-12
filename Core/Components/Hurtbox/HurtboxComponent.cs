using Godot;
using RotOfTime.Core.Combat.Attacks;

namespace RotOfTime.Core.Components.Hurtbox;

/// <summary>
///     Hurtbox component for receiving attacks.
///     Reads AttackResult from any IAttack and signals for damage processing.
/// </summary>
public partial class HurtboxComponent : Area2D
{
    [Signal]
    public delegate void AttackReceivedEventHandler(AttackResult attackResult);

    /// <summary>
    ///     Called by attacks to trigger damage.
    /// </summary>
    public void ReceiveAttack(IAttack attack)
    {
        var targetId = GetInstanceId();
        if (!attack.CanHit(targetId)) return;

        attack.RegisterHit(targetId);
        EmitSignal(SignalName.AttackReceived, attack.AttackResult);
    }

    private void OnHurtboxAreaEntered(Area2D area)
    {
        if (area is not IAttack attack) return;

        var targetId = GetInstanceId();
        if (!attack.CanHit(targetId)) return;

        attack.RegisterHit(targetId);
        EmitSignal(SignalName.AttackReceived, attack.AttackResult);
    }
}
