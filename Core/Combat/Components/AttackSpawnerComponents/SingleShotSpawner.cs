using Godot;
using RotOfTime.Core.Combat.Attacks;
using RotOfTime.Core.Entities;

namespace RotOfTime.Core.Combat.Components.AttackSpawnerComponents;

[GlobalClass]
public partial class SingleShotSpawner : AttackSpawnerComponent
{
    private bool _hasSpawned;

    public override bool IsComplete => _hasSpawned;

    public override void Activate(Vector2 direction, Vector2 position, EntityStats ownerStats, AttackData attackData,
        Node2D ownerNode)
    {
        SpawnAttackInstance(direction, position, ownerStats, attackData);
        _hasSpawned = true;
    }

    public override void Process(double delta)
    {
    }

    public override void Reset()
    {
        _hasSpawned = false;
    }
}
