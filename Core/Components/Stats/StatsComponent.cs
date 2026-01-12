using System;
using Godot;
using RotOfTime.Core.Combat.Attacks;
using RotOfTime.Core.Entities;

namespace RotOfTime.Core.Components.Stats;

public partial class StatsComponent : Node
{
    [Signal]
    public delegate void DiedEventHandler();

    [Signal]
    public delegate void HealthChangedEventHandler();

    [Signal]
    public delegate void InvincibilityEndedEventHandler();

    [Signal]
    public delegate void InvincibilityStartedEventHandler();

    [Signal]
    public delegate void StatsUpdatedEventHandler();

    private bool _invincibility;

    [Export] public int CurrentHealthPoints { get; set; } = 100;
    [Export] public float InvincibilityDuration { get; set; } = 1.0f;
    [Export] public EntityStats EntityStats { get; set; }
    [Export] public Timer InvincibilityTimer { get; set; }

    public int MaxHealthPoints { get; private set; }

    public override void _Ready()
    {
        RecalculateStats();
        SetupInvincibility();
    }

    private void RecalculateStats()
    {
        MaxHealthPoints = EntityStats.MaxHealth;
    }

    public void UpdateBaseStats(EntityStats newStats)
    {
        EntityStats = newStats;
        RecalculateStats();
        EmitSignal(SignalName.StatsUpdated);
    }

    #region Health Management

    public void TakeDamage(DamageResult damage)
    {
        if (_invincibility) return;

        CurrentHealthPoints = Math.Max(0, CurrentHealthPoints - damage.FinalDamage);

        EmitSignal(SignalName.HealthChanged);
        if (CurrentHealthPoints <= 0)
            EmitSignal(SignalName.Died);

        if (InvincibilityDuration > 0)
            StartInvincibility();
    }

    private void SetupInvincibility()
    {
        InvincibilityTimer.WaitTime = InvincibilityDuration;
        InvincibilityTimer.Timeout += OnInvincibilityEnded;

        _invincibility = false;
    }

    private void StartInvincibility()
    {
        _invincibility = true;
        InvincibilityTimer.Start(InvincibilityDuration);
        EmitSignal(SignalName.InvincibilityStarted);
    }

    private void OnInvincibilityEnded()
    {
        EmitSignal(SignalName.InvincibilityEnded);
        _invincibility = false;
    }

    #endregion
}
