using System;
using Godot;
using RotOfTime.Core.Combat.Results;
using RotOfTime.Core.Economy;
using RotOfTime.Core.Entities.Components;
using RotOfTime.Scenes.Enemies.BasicEnemy.Components;
using HurtboxComponent = RotOfTime.Core.Combat.Components.HurtboxComponent;

namespace RotOfTime.Scenes.Enemies.BasicEnemy;

public partial class BasicEnemy : CharacterBody2D
{
    [Export] public EntityStatsComponent EntityStatsComponent;
    [Export] public HurtboxComponent HurtboxComponent;
    [Export] public EnemyAttackManager AttackManager;
    [Export] public float Speed { get; set; } = 50f;
    [Export] public float AttackRange { get; set; } = 150f;
    [Export] public int IsotopeDropAmount { get; set; } = 10;
    [Export] public Area2D DetectionArea { get; set; }

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
    }

    public override void _PhysicsProcess(double delta)
    {
        // Movement logic delegated to StateMachine
    }

    /// <summary>
    ///     Called by AttackingState to fire a ranged attack toward the player.
    /// </summary>
    public void TryAttack(Vector2 direction)
    {
        if (AttackManager == null) return;

        AttackManager.TryFire(
            EnemyAttackSlot.RangedAttack,
            direction,
            GlobalPosition,
            EntityStatsComponent.EntityStats,
            this
        );
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
        SpawnIsotopeDrop();
        QueueFree();
    }

    private void SpawnIsotopeDrop()
    {
        if (IsotopeDropAmount <= 0) return;

        var pickupScene = GD.Load<PackedScene>("res://Core/Economy/IsotopePickup.tscn");
        if (pickupScene == null)
        {
            GD.PrintErr("BasicEnemy: IsotopePickup.tscn not found");
            return;
        }

        var pickup = pickupScene.Instantiate<IsotopePickup>();
        pickup.Amount = IsotopeDropAmount;
        pickup.GlobalPosition = GlobalPosition;
        GetParent().CallDeferred(Node.MethodName.AddChild, pickup);
    }

    #endregion
}
