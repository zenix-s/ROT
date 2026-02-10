using System;
using Godot;
using RotOfTime.Core.Combat.Results;
using RotOfTime.Core.Entities.Components;
using HurtboxComponent = RotOfTime.Core.Combat.Components.HurtboxComponent;

namespace RotOfTime.Scenes.Enemies.BasicEnemy;

public partial class BasicEnemy : CharacterBody2D
{
    [Export] public EntityStatsComponent EntityStatsComponent;
    [Export] public HurtboxComponent HurtboxComponent;
    [Export] public float Speed { get; set; } = 50f;
    [Export] public Area2D DetectionArea { get; set; }

    // TODO: Add EnemyAttackManager : AttackManagerComponent<EnemyAttackSlot> when enemy attacks are implemented

    public Node2D Target { get; private set; }

    public override void _Ready()
    {
        SetupStatsComponent();
        SetupHurtboxComponent();

        if (DetectionArea != null)
        {
            DetectionArea.BodyEntered += OnDetectionAreaBodyEntered;
            DetectionArea.BodyExited += OnDetectionAreaBodyExited;
        }

        // TODO: Register enemy attacks when attack system supports body attacks
    }

    public override void _PhysicsProcess(double delta)
    {
        // Movement logic delegated to StateMachine
    }

    private void OnDetectionAreaBodyEntered(Node2D body)
    {
        if (body is Player.Player)
            Target = body;
    }

    private void OnDetectionAreaBodyExited(Node2D body)
    {
        if (body == Target)
            Target = null;
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
    }

    private void SetupStatsComponent()
    {
        if (EntityStatsComponent == null)
            throw new InvalidOperationException("StatsComponent is not set");

        EntityStatsComponent.EntityDied += OnEnemyDied;
        EntityStatsComponent.HealthChanged += OnEnemyHealthChanged;
    }

    #region Event Handlers

    private void OnEnemyHealthChanged(int newHealth)
    {
        // TODO: Update enemy health display
    }

    private void OnEnemyDied()
    {
        QueueFree();
    }

    #endregion
}
