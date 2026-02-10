using System;
using Godot;
using RotOfTime.Autoload;
using RotOfTime.Core.Animation.Components;
using RotOfTime.Core.Combat.Results;
using RotOfTime.Core.Entities.Components;
using RotOfTime.Scenes.Player.Components;
using HurtboxComponent = RotOfTime.Core.Combat.Components.HurtboxComponent;

namespace RotOfTime.Scenes.Player;

public partial class Player : CharacterBody2D
{
    public const float Speed = 200.0f;

    [Export] public AnimationComponent AnimationComponent;
    [Export] public Label DebugLabel;
    [Export] public EntityStatsComponent EntityStatsComponent;
    [Export] public EntityInputComponent EntityInputComponent;
    [Export] public EntityMovementComponent EntityMovementComponent;
    [Export] public HurtboxComponent HurtboxComponent;
    [Export] public PlayerAttackManager AttackManager;

    public PlayerAttackSlot? ActiveAttackSlot { get; set; }

    public override void _Ready()
    {
        SetupStatsComponent();
        SetupHurtboxComponent();
    }

    public override void _Process(double delta)
    {
        DebugLabel.Text =
            $"Health: {EntityStatsComponent.CurrentHealth}/{EntityStatsComponent.EntityStats.VitalityStat}\n";
    }

    private void SetupHurtboxComponent()
    {
        if (HurtboxComponent == null)
            throw new InvalidOperationException("HurtboxComponent is not set");

        HurtboxComponent.AttackReceived += OnAttackReceived;
    }

    private void OnAttackReceived(AttackResult attackResult)
    {
        EntityStatsComponent.TakeDamage(attackResult);
        AnimationComponent.Blink(new Color("#FF0000"), 0.5);
        HurtboxComponent.StartInvincibility(0.5f);
    }

    private void SetupStatsComponent()
    {
        if (EntityStatsComponent == null)
            throw new InvalidOperationException("StatsComponent is not set");
        EntityStatsComponent.EntityDied += OnPlayerDied;
        EntityStatsComponent.HealthChanged += OnPlayerHealthChanged;
    }

    #region Event Handlers

    private void OnPlayerHealthChanged(int newHealth)
    {
        // TODO: Notify HUD to update health display
    }

    private void OnPlayerDied()
    {
        GameManager.Instance.PlayerDied();
    }

    #endregion
}
