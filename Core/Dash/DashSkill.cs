using Godot;

namespace RotOfTime.Core.Dash;

/// <summary>
///     Base para todos los tipos de dash intercambiables.
///     Mini-jerarquía paralela a ActiveSkill — NO usa AttackContext.
///     Timer interno gestiona cooldown igual que ActiveSkill.
/// </summary>
public abstract partial class DashSkill : Node2D
{
    [Export] public DashData Data { get; set; }

    private Timer _timer;

    public override void _EnterTree()
    {
        _timer = new Timer { OneShot = true };
        AddChild(_timer);
    }

    public bool IsReady => _timer.IsStopped();

    public float GetCooldownProgress() =>
        _timer.IsStopped() ? 0f : Mathf.Clamp((float)(_timer.TimeLeft / _timer.WaitTime), 0f, 1f);

    /// <summary>
    ///     Intenta ejecutar el dash. Devuelve false si está en cooldown.
    ///     Si ejecuta: aplica velocidad al owner y arranca cooldown.
    /// </summary>
    public bool TryExecute(Vector2 direction, Node2D owner)
    {
        if (!IsReady || Data == null) return false;
        Execute(direction, owner);
        if (Data.Cooldown > 0f) _timer.Start(Data.Cooldown);
        return true;
    }

    protected abstract void Execute(Vector2 direction, Node2D owner);
}
