using Godot;
using RotOfTime.Core.Combat.Attacks;
using RotOfTime.Core.Combat.Components;

namespace RotOfTime.Scenes.Enemies.BasicEnemy.Components;

/// <summary>
///     Enemy-specific attack manager. Manages a single body attack slot.
///     Mirrors PlayerAttackManager pattern: exports AttackData, registers slot + timer.
/// </summary>
[GlobalClass]
public partial class EnemyAttackManager : AttackManagerComponent<EnemyAttackSlot>
{
    [Export] public AttackData BodyAttackData { get; set; }

    public override void _Ready()
    {
        if (BodyAttackData != null)
        {
            RegisterSlot(EnemyAttackSlot.BodyAttack, BodyAttackData);
            var timer = new Timer
            {
                OneShot = true,
                Name = "BodyAttackTimer"
            };
            AddChild(timer);
            RegisterTimer(EnemyAttackSlot.BodyAttack, timer);
        }
    }
}
