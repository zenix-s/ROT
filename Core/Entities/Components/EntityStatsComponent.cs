using System;
using Godot;
using RotOfTime.Core.Combat.Calculations;
using RotOfTime.Core.Combat.Results;

namespace RotOfTime.Core.Entities.Components;

[GlobalClass]
public partial class EntityStatsComponent : Node
{
    [Signal]
    public delegate void EntityDiedEventHandler();

    [Signal]
    public delegate void HealthChangedEventHandler(int newHealth);

    [Signal]
    public delegate void StatsUpdatedEventHandler();

    [Export] public EntityStats EntityStats;

    public int CurrentHealth { get; private set; }

    /// <summary>Set by the entity coordinator (e.g. Player.cs). Defaults to 1.0 (no bonus).</summary>
    public float HealthMultiplier { get; set; } = 1.0f;

    /// <summary>Set by the entity coordinator (e.g. Player.cs). Defaults to 1.0 (no bonus).</summary>
    public float DamageMultiplier { get; set; } = 1.0f;

    /// <summary>Max health factoring in external multipliers.</summary>
    public int MaxHealth => Mathf.RoundToInt(EntityStats.VitalityStat * HealthMultiplier);

    /// <summary>Attack power factoring in external multipliers.</summary>
    public int AttackPower => Mathf.RoundToInt(EntityStats.AttackStat * DamageMultiplier);

    public override void _Ready()
    {
        if (EntityStats == null)
            throw new InvalidOperationException("EntityStats is not set");

        SetupHealth();
    }

    private void SetupHealth()
    {
        CurrentHealth = MaxHealth;
    }

    public void TakeDamage(AttackResult attackResult)
    {
        DamageResult damageResult = DamageCalculator.CalculateFinalDamage(attackResult, EntityStats);

        CurrentHealth = Math.Max(0, CurrentHealth - damageResult.FinalDamage);
        EmitSignal(SignalName.HealthChanged, CurrentHealth);

        if (CurrentHealth <= 0)
            EmitSignal(SignalName.EntityDied);
    }

    /// <summary>
    /// Recalculates MaxHealth and clamps CurrentHealth. Call after multipliers change.
    /// </summary>
    public void RecalculateStats()
    {
        int newMax = MaxHealth;
        if (CurrentHealth > newMax)
            CurrentHealth = newMax;

        EmitSignal(SignalName.StatsUpdated);
    }

    public void ResetStats()
    {
        CurrentHealth = MaxHealth;
    }
}
