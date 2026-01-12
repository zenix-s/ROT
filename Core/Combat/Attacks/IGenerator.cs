using Godot;
using RotOfTime.Core.Entities;
using RotOfTime.Core.Grimoire;

namespace RotOfTime.Core.Combat.Attacks;

/// <summary>
///     Interface for attack generators.
///     Generators spawn attack instances with a cooldown.
/// </summary>
public interface IGenerator
{
    float SpawnCooldown { get; }
    PackedScene AttackPrefab { get; }
    void UpdateStats(EntityStats entity, GrimoireStats grimoire);
}
