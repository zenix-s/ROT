using RotOfTime.Core.Combat.Projectiles;
using RotOfTime.Core.Entities;

namespace RotOfTime.Core.Combat.Data;

public class AttackData
{
    public string Name { get; set; } = "Unnamed Attack";
    public float DamageCoefficient { get; set; } = 1.0f;
    public ProjectileSettings ProjectileSettings { get; set; }

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
