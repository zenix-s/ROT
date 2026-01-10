using System;
using Godot;
using RotOfTime.Core.Components.Stats;

namespace RotOfTime.Scenes.Player;

/// <summary>
///     Player component manager, coordinates player components.
/// </summary>
public partial class ComponentManager : Node
{
    [Export] public StatsComponent StatsComponent;

    public override void _Ready()
    {
        SetupStatsComponent();
    }

    private void SetupStatsComponent()
    {
        if (StatsComponent == null)
            throw new InvalidOperationException("StatsComponent is not set");

        StatsComponent.Died += OnPlayerDied;
        StatsComponent.HealthChanged += OnPlayerHealthChanged;
        StatsComponent.InvincibilityStarted += OnPlayerInvincibilityStarted;
        StatsComponent.InvincibilityEnded += OnPlayerInvincibilityEnded;
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
