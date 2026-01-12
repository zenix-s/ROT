using Godot;
using RotOfTime.Core.Combat.Calculations;
using RotOfTime.Core.Entities;
using RotOfTime.Core.Grimoire;

namespace RotOfTime.Core.Combat.Attacks.Generators;

/// <summary>
///     Base class for attack generators.
///     Handles spawn cooldown and stats propagation to spawned attacks.
/// </summary>
public abstract partial class BaseGenerator : Node2D, IGenerator
{
    [Export] public float SpawnCooldown { get; set; } = 1.0f;
    [Export] public PackedScene AttackPrefab { get; set; }

    protected EntityStats EntityStats;
    protected GrimoireStats GrimoireStats;
    protected float CooldownTimer;
    protected bool CanSpawn => CooldownTimer <= 0;

    public override void _Ready()
    {
        CooldownTimer = 0;
    }

    public override void _Process(double delta)
    {
        if (CooldownTimer > 0)
            CooldownTimer -= (float)delta;

        if (CanSpawn)
            TrySpawn();
    }

    public void UpdateStats(EntityStats entity, GrimoireStats grimoire)
    {
        EntityStats = entity;
        GrimoireStats = grimoire;
    }

    protected virtual void TrySpawn()
    {
        if (AttackPrefab == null) return;

        var attack = AttackPrefab.Instantiate();
        if (attack is IAttack attackInstance)
        {
            attackInstance.UpdateStats(EntityStats, GrimoireStats);
        }

        ConfigureSpawnedAttack(attack);
        GetTree().CurrentScene.AddChild(attack);

        CooldownTimer = SpawnCooldown;
    }

    /// <summary>
    ///     Override to configure the spawned attack (position, direction, etc.)
    /// </summary>
    protected abstract void ConfigureSpawnedAttack(Node attack);
}
