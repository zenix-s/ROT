using System;
using Godot;
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

    /// <summary>
    ///     Bonus plano de HP en unidades HP (múltiplos de 10).
    ///     Sumado a EntityStats.VitalityStat para obtener MaxHealth.
    ///     Fuentes: progresión (elevaciones) + artefactos.
    /// </summary>
    public int MaxHealthBonus { get; set; } = 0;

    /// <summary>Multiplicador de daño. Aplicado al AttackStat al disparar.</summary>
    public float DamageMultiplier { get; set; } = 1.0f;

    /// <summary>HP máximo = vida base + bonus de elevaciones y artefactos.</summary>
    public int MaxHealth => EntityStats.VitalityStat + MaxHealthBonus;

    /// <summary>Attack power efectivo con multiplicador aplicado.</summary>
    public int AttackPower => Mathf.RoundToInt(EntityStats.AttackStat * DamageMultiplier);

    public override void _Ready()
    {
        if (EntityStats == null)
            throw new InvalidOperationException("EntityStats is not set");

        CurrentHealth = MaxHealth;
    }

    public void TakeDamage(AttackResult attackResult)
    {
        // Sin defensa — el daño del AttackResult ya es el daño final.
        CurrentHealth = Math.Max(0, CurrentHealth - attackResult.RawDamage);
        EmitSignal(SignalName.HealthChanged, CurrentHealth);

        if (CurrentHealth <= 0)
            EmitSignal(SignalName.EntityDied);
    }

    /// <summary>
    ///     Recalcula MaxHealth y clampea CurrentHealth. Llamar tras cambiar MaxHealthBonus.
    /// </summary>
    public void RecalculateStats()
    {
        if (CurrentHealth > MaxHealth)
            CurrentHealth = MaxHealth;

        EmitSignal(SignalName.StatsUpdated);
    }

    public void ResetStats()
    {
        CurrentHealth = MaxHealth;
    }
}
