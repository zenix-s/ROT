using System;
using Godot;
using RotOfTime.Autoload;
using RotOfTime.Core.Combat.Results;
using RotOfTime.Core.Components;
using RotOfTime.Scenes.Attacks.Projectiles;

namespace RotOfTime.Scenes.Player;

public partial class Player : CharacterBody2D
{
    public const float Speed = 200.0f;

    private float _attackTimer;
    private bool _canAttack = true;

    [Export] public AnimationComponent AnimationComponent;
    [Export] public float AttackCooldown = 0.3f;
    [Export] public Label DebugLabel;
    [Export] public EntityStatsComponent EntityStatsComponent;
    [Export] public HurtboxComponent HurtboxComponent;
    [Export] public PackedScene ProjectileScene;

    public override void _Ready()
    {
        SetupStatsComponent();
        SetupHurtboxComponent();
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

        HandleAttack(delta);

        DebugLabel.Text =
            $"Health: {EntityStatsComponent.CurrentHealth}/{EntityStatsComponent.EntityStats.VitalityStat}\n";
    }

    private void HandleAttack(double delta)
    {
        if (!_canAttack)
        {
            _attackTimer -= (float)delta;
            if (_attackTimer <= 0)
                _canAttack = true;
        }

        if (Input.IsActionJustPressed("attack") && _canAttack && ProjectileScene != null)
        {
            GD.Print("Player Attack!");
            Vector2 mousePosition = GetGlobalMousePosition();
            Vector2 directionToMouse = (mousePosition - GlobalPosition).Normalized();

            Projectile projectile = ProjectileScene.Instantiate<Projectile>();
            Vector2 spawnOffset = directionToMouse * 16; // Spawn a bit in front of the player
            projectile.Execute(
                direction: directionToMouse,
                position: GlobalPosition + spawnOffset,
                ownerStats: EntityStatsComponent.EntityStats);
            GetParent().AddChild(projectile);

            _canAttack = false;
            _attackTimer = AttackCooldown;
        }
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
