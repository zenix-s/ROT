using Godot;
using RotOfTime.Core.Combat.Attacks;

namespace RotOfTime.Core.Combat.Skills;

public abstract partial class ActiveSkill : Skill
{
    private Timer _timer;

    public override void _EnterTree()
    {
        _timer = new Timer { OneShot = true };
        AddChild(_timer);
    }

    public bool IsReady => _timer.IsStopped();

    public float GetCooldownProgress() =>
        _timer.IsStopped() ? 0f : Mathf.Clamp((float)(_timer.TimeLeft / _timer.WaitTime), 0f, 1f);

    public abstract bool TryExecute(AttackContext ctx);

    protected void StartCooldown(float duration)
    {
        if (duration > 0f) _timer.Start(duration);
    }
}
