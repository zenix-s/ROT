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
    private EntityStats _entityStats;
    [Export] public Array<Node2D> Attacks = new();
    [Export] public Array<Node2D> Generators = new();
    [Export] public GrimoireStats Stats;

    public void UpdateEntityStats(EntityStats stats)
    {
        _entityStats = stats;
        UpdateAll();
    }

    private void UpdateAll()
    {
        foreach (Node2D node in Attacks)
            if (node is IAttack attack)
                attack.UpdateStats(_entityStats, Stats);

        foreach (Node2D node in Generators)
            if (node is IGenerator generator)
                generator.UpdateStats(_entityStats, Stats);
    }
}
