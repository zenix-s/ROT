using Godot;
using RotOfTime.Core.Combat.Attacks;
using RotOfTime.Core.Entities;

namespace RotOfTime.Core.Combat.Components.AttackSpawnerComponents;

/// <summary>
///     Invoca multiples ataques en burst desde una posicion fija.
/// </summary>
[GlobalClass]
public partial class TurretSpawner : AttackSpawnerComponent
{
    private AttackData _attackData;
    private Vector2 _fixedDirection;
    private Vector2 _fixedPosition;
    private EntityStats _ownerStats;
    private int _spawned;
    private float _timeSinceLastSpawn;

    public int Count { get; set; } = 3;
    public float DelayBetweenShots { get; set; } = 0.25f;
    
    public override bool IsComplete => _spawned >= Count;

    public override void Activate(Vector2 direction, Vector2 position, EntityStats ownerStats, AttackData attackData,
        Node2D ownerNode)
    {
        _fixedDirection = direction.Normalized();
        _fixedPosition = position;
        _ownerStats = ownerStats;
        _attackData = attackData;
        
        SpawnAttackInstance(_fixedDirection, _fixedPosition, _ownerStats, _attackData);
        _spawned = 1;
        _timeSinceLastSpawn = 0f;
    }

    public override void Process(double delta)
    {
        if (IsComplete)
            return;

        _timeSinceLastSpawn += (float)delta;

        while (_timeSinceLastSpawn >= DelayBetweenShots && _spawned < Count)
        {
            SpawnAttackInstance(_fixedDirection, _fixedPosition, _ownerStats, _attackData);
            _spawned++;
            _timeSinceLastSpawn -= DelayBetweenShots;
        }
    }

    public override void Reset()
    {
        _spawned = 0;
        _timeSinceLastSpawn = 0f;
        _fixedDirection = Vector2.Zero;
        _fixedPosition = Vector2.Zero;
        _ownerStats = null;
        _attackData = null;
    }
}
