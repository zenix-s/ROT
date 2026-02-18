using RotOfTime.Core.Combat.Components.AttackMovementComponents;

namespace RotOfTime.Core.Combat.Attacks;

public static class MovementFactory
{
    public static AttackMovementComponent Create(MovementType type)
    {
        return type switch
        {
            MovementType.Linear => new LinearMovementComponent(),
            _ => new LinearMovementComponent()
        };
    }
}
