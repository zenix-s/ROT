using RotOfTime.Core.Components;
using RotOfTime.Core.Entities;

namespace RotOfTime.Core.Combat.Attacks;

/// <summary>
///     Interface for any node that acts as an attack.
///     Implement this on the root node of your attack scene.
/// </summary>
public interface IAttack
{
    /// <summary>
    ///     The damage component that handles damage calculation and hit tracking.
    /// </summary>
    AttackDamageComponent DamageComponent { get; }

    /// <summary>
    ///     Update the attack's damage based on the owner's stats.
    /// </summary>
    void UpdateStats(EntityStats ownerStats);
}
