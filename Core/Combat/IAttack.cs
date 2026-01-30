using Godot;
using RotOfTime.Core.Entities;
using AttackDamageComponent = RotOfTime.Core.Combat.Components.AttackDamageComponent;
using AttackHitboxComponent = RotOfTime.Core.Combat.Components.AttackHitboxComponent;

namespace RotOfTime.Core.Combat;

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
    ///     The hitbox component that detects collisions with hurtboxes.
    /// </summary>
    AttackHitboxComponent HitboxComponent { get; }

    /// <summary>
    ///     Update the attack's damage based on the owner's stats.
    /// </summary>
    void UpdateStats(EntityStats ownerStats);

    /// <summary>
    ///     Execute the attack from a given position in a given direction.
    /// </summary>
    void Execute(Vector2 direction, Vector2 position, EntityStats ownerStats);
}
