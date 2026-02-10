using Godot;
using RotOfTime.Core.Entities;

namespace RotOfTime.Core.Combat.Components.AttackSpawnerComponents;

/// <summary>
///     Spawns a single attack immediately on activation.
///     Replaces SingleShotStrategy.
/// </summary>
[GlobalClass]
public partial class SingleShotSpawner : AttackSpawnerComponent
{
    private bool _hasSpawned;

    public override bool IsComplete => _hasSpawned;

    public override void Activate(Vector2 direction, Vector2 position, EntityStats ownerStats)
    {
        SpawnAttackInstance(direction, position, ownerStats);
        _hasSpawned = true;
    }

    public override void Process(double delta)
    {
        // Single shot spawns everything in Activate, nothing to do here.
    }

    public override void Reset()
    {
        _hasSpawned = false;
    }
}
