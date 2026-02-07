using System;
using Godot;
using RotOfTime.Core.Entities;

namespace RotOfTime.Core.Combat;

/// <summary>
///     Interface for any node that acts as an attack.
///     Implement this on the root node of your attack scene.
/// </summary>
public interface IAttack
{
    /// <summary>
    ///     The cooldown duration in seconds before this attack can be used again.
    /// </summary>
    float Cooldown { get; }

    /// <summary>
    ///     If true, the attack fires instantly without entering CastingState.
    /// </summary>
    bool IsInstantCast { get; }

    /// <summary>
    ///     If true, the player can move during the cast phase.
    ///     Only relevant for non-instant attacks.
    /// </summary>
    bool AllowMovementDuringCast { get; }

    /// <summary>
    ///     Emitted when the cast phase is complete and the player can act again.
    /// </summary>
    event Action CastCompleted;

    /// <summary>
    ///     Emitted when the entire attack lifecycle is done.
    /// </summary>
    event Action AttackFinished;

    /// <summary>
    ///     Execute the attack from a given position in a given direction.
    /// </summary>
    void Execute(Vector2 direction, Vector2 position, EntityStats ownerStats);
}
