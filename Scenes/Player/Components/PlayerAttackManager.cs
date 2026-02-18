using Godot;
using RotOfTime.Core.Combat.Components;

namespace RotOfTime.Scenes.Player.Components;

[GlobalClass]
public partial class PlayerAttackManager : AttackManagerComponent<PlayerAttackSlot>
{
    [Export] public PackedScene BasicAttackSkill { get; set; }
    [Export] public PackedScene Spell1Skill { get; set; }
    [Export] public PackedScene Spell2Skill { get; set; }

    public override void _Ready()
    {
        if (BasicAttackSkill != null)
            RegisterSkill(PlayerAttackSlot.BasicAttack, BasicAttackSkill);

        if (Spell1Skill != null)
            RegisterSkill(PlayerAttackSlot.Spell1, Spell1Skill);

        if (Spell2Skill != null)
            RegisterSkill(PlayerAttackSlot.Spell2, Spell2Skill);
    }
}
