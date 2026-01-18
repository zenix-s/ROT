using System;
using Godot;
using RotOfTime.Core.Combat.Data;
using RotOfTime.Core.Components;
using RotOfTime.Core.Entities;

namespace RotOfTime.Scenes.Enemies.BasicEnemy;

public partial class BasicEnemy : CharacterBody2D
{
    private IAttack _bodyAttack;
    private Node2D _target;

    [Export] public EntityStatsComponent EntityStatsComponent;
    [Export] public HurtboxComponent HurtboxComponent;
    [Export] public float Speed { get; set; } = 50f;
    [Export] public Area2D DetectionArea { get; set; }
    [Export] public Node2D BodyAttackNode { get; set; }

    public override void _Ready()
    {
        SetupStatsComponent();
        SetupHurtboxComponent();

        if (DetectionArea != null)
        {
            DetectionArea.BodyEntered += OnDetectionAreaBodyEntered;
            DetectionArea.BodyExited += OnDetectionAreaBodyExited;
        }

        if (BodyAttackNode is IAttack attack)
        {
            _bodyAttack = attack;
            _bodyAttack.UpdateStats(EntityStatsComponent.EntityStats);
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
        if (body is Player.Player)
            _target = body;
    }

    private void OnDetectionAreaBodyExited(Node2D body)
    {
        if (body == _target)
            _target = null;
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
