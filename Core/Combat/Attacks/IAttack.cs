using Godot;
using RotOfTime.Core.Entities;

namespace RotOfTime.Core.Combat.Attacks;

/// <summary>
///     Common interface for all attack instances (projectiles, body attacks, AoE, etc.).
///     Any scene that can be spawned by an AttackSpawnerComponent must implement this.
///     Implementors are expected to be Node2D subclasses.
///     The attack scene has no knowledge of its own stats; everything is injected via Execute.
///     Positioning is handled by the spawner before Execute is called.
/// </summary>
public interface IAttack
{
    /// <summary>
    ///     Initialize and activate the attack with the given parameters.
    ///     The node is already positioned in the scene tree when this is called.
    /// </summary>
    /// <param name="direction">Normalized direction the attack is aimed at</param>
    /// <param name="ownerStats">Stats of the entity that initiated the attack</param>
    /// <param name="attackData">Attack definition with damage coefficient and other stats</param>
    /// <param name="damageMultiplier">Multiplier from the spawner pattern (default 1.0)</param>
    void Execute(Vector2 direction, EntityStats ownerStats, AttackData attackData,
        float damageMultiplier = 1.0f);
}
