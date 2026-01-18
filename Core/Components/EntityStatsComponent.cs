using System;
using Godot;
using RotOfTime.Core.Combat.Attacks;
using RotOfTime.Core.Combat.Calculations;
using RotOfTime.Core.Entities;

namespace RotOfTime.Core.Components;

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

    public override void _Ready()
    {
        if (EntityStats == null)
            throw new InvalidOperationException("EntityStats is not set");

        SetupHealth();
    }

    private void SetupHealth()
    {
        CurrentHealth = EntityStats.VitalityStat;
    }


    public void TakeDamage(AttackResult attackResult)
    {
        DamageResult damageResult = DamageCalculator.CalculateFinalDamage(attackResult, EntityStats);
        GD.Print(
            $"Taking damage: {damageResult.RawDamage} raw, {damageResult.FinalDamage} final (after {EntityStats.DefenseStat} defense)");

        CurrentHealth = Math.Max(0, CurrentHealth - damageResult.FinalDamage);
        EmitSignal(SignalName.HealthChanged, CurrentHealth);

        if (CurrentHealth <= 0)
            EmitSignal(SignalName.EntityDied);
    }

    public void ResetStats()
    {
        CurrentHealth = EntityStats.VitalityStat;
    }
}
