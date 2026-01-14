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
}

public static class EntityStatsCombatExtensions
{
    public static DamageResult CalculateDamageTaken(this IEntityStats defenderStats, AttackResult attackResult)
    {
        int rawDamage = attackResult.RawDamage;
        int defense = defenderStats.DefenseStat;

        int finalDamage = Math.Max(0, rawDamage - defense);

        return new DamageResult(
            rawDamage,
            finalDamage,
            attackResult.IsCritical,
            false, // TODO: Implement block logic
            rawDamage - finalDamage
        );
    }

    public static void TakeDamage(this EntityStats defenderStats, DamageResult damage)
    {
        defenderStats.CurrentHealth = Math.Max(0, defenderStats.CurrentHealth - damage.FinalDamage);
        defenderStats.EmitSignal(nameof(EntityStats.HealthChanged), defenderStats.CurrentHealth);

        if (defenderStats.CurrentHealth <= 0)
            defenderStats.EmitSignal(nameof(EntityStats.EntityDied));
    }
}
