using Godot;
using RotOfTime.Core.Entities;

namespace RotOfTime.Core.Combat.Attacks;

/// <summary>
///     Common interface for all attack instances (projectiles, body attacks, AoE, etc.).
///     Any scene that can be spawned by an AttackSpawnerComponent must implement this.
///     Implementors are expected to be Node2D subclasses.
/// </summary>
public interface IAttack
{
    /// <summary>
    ///     Initialize and activate the attack with the given parameters.
    /// </summary>
    /// <param name="direction">Normalized direction the attack is aimed at</param>
    /// <param name="position">World position where the attack spawns</param>
    /// <param name="ownerStats">Stats of the entity that initiated the attack</param>
    void Execute(Vector2 direction, Vector2 position, EntityStats ownerStats);
}
