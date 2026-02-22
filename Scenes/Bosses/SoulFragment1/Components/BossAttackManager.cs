using Godot;
using RotOfTime.Core.Combat.Components;

namespace RotOfTime.Scenes.Bosses.SoulFragment1.Components;

[GlobalClass]
public partial class BossAttackManager : AttackManagerComponent<BossAttackSlot>
{
    [Export] public PackedScene RangedAttackSkill { get; set; }

    public override void _Ready()
    {
        if (RangedAttackSkill != null)
            RegisterSkill(BossAttackSlot.RangedAttack, RangedAttackSkill);
    }
}
