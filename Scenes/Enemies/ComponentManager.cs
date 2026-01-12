using System;
using Godot;
using RotOfTime.Core.Combat.Attacks;
using RotOfTime.Core.Combat.Calculations;
using RotOfTime.Core.Components.Hurtbox;
using RotOfTime.Core.Components.Stats;

namespace RotOfTime.Scenes.Enemies;

/// <summary>
///     Player component manager, coordinates player components.
/// </summary>
public partial class ComponentManager : Node
{
    [Export] public StatsComponent StatsComponent;
    [Export] public HurtboxComponent HurtboxComponent;
    [Export] public Grimoire.Grimoire Grimoire;

    public override void _Ready()
    {
        SetupStatsComponent();
        SetupHurtboxComponent();
        SetupGrimoire();
    }

    private void SetupGrimoire()
    {
        if (Grimoire == null) return;
        Grimoire.UpdateEntityStats(StatsComponent.EntityStats);
    }

    private void SetupHurtboxComponent()
    {
        if (HurtboxComponent == null)
            throw new InvalidOperationException("HurtboxComponent is not set");

        HurtboxComponent.AttackReceived += OnAttackReceived;
    }

    private void OnAttackReceived(AttackResult attackResult)
    {
        // Calculate damage using defender's stats
        DamageResult damageResult = DamageCalculator.Calculate(
            StatsComponent.EntityStats,
            attackResult
        );

        // Apply damage
        StatsComponent.TakeDamage(damageResult);
    }

    private void SetupStatsComponent()
    {
        if (StatsComponent == null)
            throw new InvalidOperationException("StatsComponent is not set");

        StatsComponent.Died += OnPlayerDied;
        StatsComponent.HealthChanged += OnPlayerHealthChanged;
        StatsComponent.InvincibilityStarted += OnPlayerInvincibilityStarted;
        StatsComponent.InvincibilityEnded += OnPlayerInvincibilityEnded;
        StatsComponent.StatsUpdated += OnStatsUpdated;
    }

    private void OnStatsUpdated()
    {
        Grimoire?.UpdateEntityStats(StatsComponent.EntityStats);
    }

    #region Event Handlers

    private void OnPlayerInvincibilityEnded()
    {
        // TODO: Update player sprite or effects
    }

    private void OnPlayerInvincibilityStarted()
    {
        // TODO: Update player sprite or effects
    }

    private void OnPlayerHealthChanged()
    {
        // TODO: Notify HUD to update health display
    }

    private void OnPlayerDied()
    {
        // TODO: Notify SceneManager to load Game Over scene
    }

    #endregion
}
