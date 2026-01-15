using RotOfTime.Core.Entities;

namespace RotOfTime.Core.Combat.Attacks;

public interface IAttack
{
    AttackResult AttackResult { get; }
    AttackData AttackData { get; }
    void UpdateStats(EntityStats entity);
}
