using Godot;
using RotOfTime.Core.Combat.Components;

namespace RotOfTime.Scenes.Enemies.BasicEnemy.Components;

[GlobalClass]
public partial class EnemyAttackManager : AttackManagerComponent<EnemyAttackSlot>
{
    [Export] public PackedScene BodyAttackSkill { get; set; }

    public override void _Ready()
    {
        if (BodyAttackSkill != null)
            RegisterSkill(EnemyAttackSlot.BodyAttack, BodyAttackSkill);
    }
}
