using System;
using Godot;
using RotOfTime.Core.Combat.Attacks;

namespace RotOfTime.Core.Entities;

[GlobalClass]
public partial class EntityStats : Resource, IEntityStats
{
    [Signal]
    public delegate void EntityDiedEventHandler();

    [Signal]
    public delegate void HealthChangedEventHandler(int newHealth);

    [Signal]
    public delegate void StatsUpdatedEventHandler();

    public EntityStats(int vitalityStat, int attackStat, int defenseStat)
    {
        VitalityStat = vitalityStat;
        AttackStat = attackStat;
        DefenseStat = defenseStat;

        CallDeferred(nameof(SetupHealth));
    }

    public EntityStats() : this(0, 0, 0)
    {
    }

    public int CurrentHealth { get; set; }

    [Export] public int VitalityStat { get; set; }
    [Export] public int AttackStat { get; set; }
    [Export] public int DefenseStat { get; set; }

    private void SetupHealth()
    {
        CurrentHealth = VitalityStat;
    }

    public void TakeDamage(AttackResult attackResult)
    {
        int finalDamage = Math.Max(0, attackResult.RawDamage - DefenseStat);
        CurrentHealth = Math.Max(0, CurrentHealth - finalDamage);
        EmitSignal(nameof(HealthChanged), CurrentHealth);

        if (CurrentHealth <= 0)
            EmitSignal(nameof(EntityDied));
    }

    public void ResetStats()
    {
        CurrentHealth = VitalityStat;
    }
}
