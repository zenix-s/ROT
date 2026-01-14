using System;
using Godot;
using RotOfTime.Core.Combat.Attacks;
using RotOfTime.Core.Components.Hurtbox;
using RotOfTime.Core.Entities;

namespace RotOfTime.Scenes.Player;

public partial class Player : CharacterBody2D
{
    public const float Speed = 200.0f;

    [Export] public AnimationComponent AnimationComponent;

    [Export] public Label DebugLabel;
    [Export] public EntityStats EntityStats;
    [Export] public Grimoire.Grimoire Grimoire;
    [Export] public HurtboxComponent HurtboxComponent;

    public override void _Ready()
    {
        SetupStatsComponent();
        SetupHurtboxComponent();
        SetupGrimoire();
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 direction = Input.GetVector(
            "move_left",
            "move_right",
            "move_top",
            "move_down");

        if (direction.X > 0)
            AnimationComponent.AnimatedSprite2D.FlipH = false;
        else if (direction.X < 0)
            AnimationComponent.AnimatedSprite2D.FlipH = true;


        Velocity = direction.Normalized() * Speed;
        MoveAndSlide();

        DebugLabel.Text =
            $"Health: {EntityStats.CurrentHealth}/{EntityStats.VitalityStat}\n";
    }

    private void SetupGrimoire()
    {
        Grimoire?.UpdateEntityStats(EntityStats);
    }

    private void SetupHurtboxComponent()
    {
        if (HurtboxComponent == null)
            throw new InvalidOperationException("HurtboxComponent is not set");

        HurtboxComponent.AttackReceived += OnAttackReceived;
    }

    private void OnAttackReceived(AttackResult attackResult)
    {
        DamageResult damageResult = EntityStats.CalculateDamageTaken(attackResult);
        EntityStats.TakeDamage(damageResult);
    }

    private void SetupStatsComponent()
    {
        if (EntityStats == null)
            throw new InvalidOperationException("StatsComponent is not set");

        EntityStats.EntityDied += OnPlayerDied;
        EntityStats.HealthChanged += OnPlayerHealthChanged;
        // EntityStats.InvincibilityStarted += OnPlayerInvincibilityStarted;
        // EntityStats.InvincibilityEnded += OnPlayerInvincibilityEnded;
        EntityStats.StatsUpdated += OnEntityStatsUpdated;
    }

    private void OnEntityStatsUpdated()
    {
        Grimoire?.UpdateEntityStats(EntityStats);
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

    private void OnPlayerHealthChanged(int newHealth)
    {
        // TODO: Notify HUD to update health display
    }

    private void OnPlayerDied()
    {
        // TODO: Notify SceneManager to load Game Over scene
    }

    #endregion
}
