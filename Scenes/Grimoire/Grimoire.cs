using Godot;
using Godot.Collections;
using RotOfTime.Core.Combat.Attacks;
using RotOfTime.Core.Entities;
using RotOfTime.Core.Grimoire;

namespace RotOfTime.Scenes.Grimoire;

/// <summary>
///     Grimoire node that owns and coordinates attacks and generators.
///     Receives EntityStats updates and combines with GrimoireStats for calculations.
/// </summary>
public partial class Grimoire : Node2D
{
    [Export] public GrimoireStats Stats;
    [Export] public Array<Node2D> Attacks = new();
    [Export] public Array<Node2D> Generators = new();

    private EntityStats _entityStats;

    public void UpdateEntityStats(EntityStats stats)
    {
        _entityStats = stats;
        UpdateAll();
    }

    private void UpdateAll()
    {
        foreach (var node in Attacks)
        {
            if (node is IAttack attack)
                attack.UpdateStats(_entityStats, Stats);
        }

        foreach (var node in Generators)
        {
            if (node is IGenerator generator)
                generator.UpdateStats(_entityStats, Stats);
        }
    }
}
