using System;
using Godot;
using RotOfTime.Core.Combat.Attacks;
using RotOfTime.Core.Combat.Calculations;
using RotOfTime.Core.Components.Hurtbox;
using RotOfTime.Core.Entities;

namespace RotOfTime.Scenes.Enemies.BasicEnemy;

/// <summary>
///     Basic enemy that moves toward the player when detected.
/// </summary>
public partial class BasicEnemy : CharacterBody2D
{
    private Node2D _target;

    [Export] public EntityStats EntityStats;
    [Export] public Grimoire.Grimoire Grimoire;
    [Export] public HurtboxComponent HurtboxComponent;
    [Export] public float Speed { get; set; } = 50f;
    [Export] public Area2D DetectionArea { get; set; }

    public override void _Ready()
    {
        SetupStatsComponent();
        SetupHurtboxComponent();
        SetupGrimoire();
        if (DetectionArea != null)
        {
            DetectionArea.BodyEntered += OnDetectionAreaBodyEntered;
            DetectionArea.BodyExited += OnDetectionAreaBodyExited;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_target == null) return;

        Vector2 direction = (_target.GlobalPosition - GlobalPosition).Normalized();
        Velocity = direction * Speed;
        MoveAndSlide();
    }

    private void OnDetectionAreaBodyEntered(Node2D body)
    {
        GD.Print("Hello");
        if (body is Player.Player player)
            _target = body;
    }

    private void OnDetectionAreaBodyExited(Node2D body)
    {
        if (body == _target)
            _target = null;
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
        // Calculate damage using defender's stats
        DamageResult damageResult = DamageCalculator.Calculate(
            EntityStats,
            attackResult
        );

        // Apply damage
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
