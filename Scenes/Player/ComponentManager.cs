using Godot;
using System;
using RotOfTime.Core.Components.Stats;


// Player component manager, coordinates player components
public partial class ComponentManager : Node
{

    [Export] public StatsComponent StatsComponent;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        SetupStatsComponent();
    }


    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    private void UpdatePlayerStats()
    {

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

    private void OnPlayerInvincibilityEnded()
    {
        // Update player sprite or effects
        throw new NotImplementedException();
    }

    private void OnPlayerInvincibilityStarted()
    {
        // Update player sprite or effects
        throw new NotImplementedException();
    }

    private void OnPlayerHealthChanged()
    {
        // Notify HUD to update health display
        throw new NotImplementedException();
    }

    private void OnPlayerDied()
    {
        // Notify SceneManager to load Game Over scene
        throw new NotImplementedException();
    }
}
