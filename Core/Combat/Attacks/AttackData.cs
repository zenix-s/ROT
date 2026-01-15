using Godot;
using RotOfTime.Core.Entities;

namespace RotOfTime.Core.Combat.Attacks;

[GlobalClass]
public partial class AttackData : Resource
{
    [Export] public string Name { get; set; } = "Unnamed Attack";
    [Export] public float DamageCoefficient { get; set; } = 1.0f;

    public AttackResult ToAttackResult(EntityStats entity)
    {
        int rawDamage = (int)(entity.AttackStat * DamageCoefficient);
        return new AttackResult(rawDamage, Name);
    }

    public AttackResult ToAttackResult()
    {
        return new AttackResult((int)DamageCoefficient, Name);
    }
}
