using Godot;
using RotOfTime.Core.Entities;

namespace RotOfTime.Core.Combat.Components.AttackSpawnerComponents;

/// <summary>
///     Spawns multiple attacks in a burst with a configurable delay between each.
///     Replaces BurstSpawnerStrategy.
/// </summary>
[GlobalClass]
public partial class BurstSpawner : AttackSpawnerComponent
{
    private Vector2 _direction;
    private EntityStats _ownerStats;
    private Vector2 _position;
    private int _spawned;
    private float _timeSinceLastSpawn;

    /// <summary>
    ///     Number of attacks to spawn in the burst.
    /// </summary>
    [Export]
    public int Count { get; set; } = 3;

    /// <summary>
    ///     Delay in seconds between each attack in the burst.
    /// </summary>
    [Export]
    public float DelayBetweenShots { get; set; } = 0.25f;

    public override bool IsComplete => _spawned >= Count;

    public override void Activate(Vector2 direction, Vector2 position, EntityStats ownerStats)
    {
        _direction = direction.Normalized();
        _position = position;
        _ownerStats = ownerStats;

        // First attack spawns immediately
        SpawnAttackInstance(_direction, _position, _ownerStats);
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
            SpawnAttackInstance(_direction, _position, _ownerStats);
            _spawned++;
            _timeSinceLastSpawn -= DelayBetweenShots;
        }
    }

    public override void Reset()
    {
        _spawned = 0;
        _timeSinceLastSpawn = 0f;
        _direction = Vector2.Zero;
        _position = Vector2.Zero;
        _ownerStats = null;
    }
}
