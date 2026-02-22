using Godot;
using RotOfTime.Core.Combat.Attacks;
using RotOfTime.Core.Combat.Components.AttackSpawnComponents;

namespace RotOfTime.Core.Combat.Skills;

[GlobalClass]
public partial class ProjectileSkill : ActiveSkill
{
    [Export] public AttackSpawnComponent Spawner { get; set; }

    public override bool TryExecute(AttackContext ctx)
    {
        if (!IsReady) return false;
        if (Spawner == null) { GD.PrintErr("ProjectileSkill: Spawner null"); return false; }
        Spawner.Execute(ctx);
        StartCooldown(Spawner.Data?.CooldownDuration ?? 0f);
        return true;
    }
}
