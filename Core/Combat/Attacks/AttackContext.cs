using Godot;
using RotOfTime.Core.Entities;

namespace RotOfTime.Core.Combat.Attacks;

/// <summary>
///     Packages all world context needed to execute an attack skill.
///     Built by AttackManagerComponent, consumed by AttackSpawnComponent and Projectile.
/// </summary>
public record AttackContext(
    Vector2 Direction,
    Vector2 SpawnPosition,
    EntityStats OwnerStats,
    Node2D Owner,
    float DamageMultiplier,
    Node AttacksContainer
);
