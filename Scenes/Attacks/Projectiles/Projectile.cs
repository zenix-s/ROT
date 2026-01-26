using System;
using Godot;
using RotOfTime.Core.Combat;
using RotOfTime.Core.Combat.Attacks;
using RotOfTime.Core.Combat.Components;
using RotOfTime.Core.Components;
using RotOfTime.Core.Entities;

namespace RotOfTime.Scenes.Attacks.Projectiles;

public partial class Projectile : CharacterBody2D, IAttack
{
    protected Vector2 Direction;
    private bool _isLaunched;
    private int _lifetime = 5;
    private Timer _lifetimeTimer;

    [Export] public AttackDamageComponent DamageComponent { get; set; }
    [Export] public AttackHitboxComponent HitboxComponent { get; set; }
    [Export] public AttackMovementComponent MovementComponent { get; set; }
    [Export] public ProjectileData AttackData { get; set; }

    public void UpdateStats(EntityStats ownerStats)
    {
        DamageComponent?.UpdateStats(ownerStats);
    }

    public void Execute(Vector2 direction, Vector2 position, EntityStats ownerStats)
    {
        GlobalPosition = position;
        Direction = direction.Normalized();
        Rotation = Direction.Angle();
        UpdateStats(ownerStats);
        _isLaunched = true;
    }

    public void ApplySettings(ProjectileData settings)
    {
        if (settings == null) return;
        AttackData = settings;
        _lifetime = settings.Lifetime;
    }

    public override void _Ready()
    {
        if (AttackData is null || DamageComponent is null || HitboxComponent is null || MovementComponent is null)
        {
            GD.PrintErr($"Projectile: Missing components or AttackData on {Name}");
            QueueFree();
            return;
        }

        _lifetime = AttackData.Lifetime;
        MovementComponent?.Initialize(AttackData);

        if (!_isLaunched)
            Direction = Vector2.Right.Rotated(GlobalRotation);

        SetupLifetimeTimer();
        SetupHitboxComponent();
    }

    public override void _PhysicsProcess(double delta)
    {
        Velocity = MovementComponent.CalculateVelocity(Velocity, Direction, delta);
        bool collision = MoveAndSlide();
        if (collision)
            OnImpact();
    }

    protected virtual void OnImpact()
    {
        QueueFree();
    }

    private void OnAttackHitboxHitConnected()
    {
        OnImpact();
    }

    private void SetupLifetimeTimer()
    {
        _lifetimeTimer = new Timer();
        _lifetimeTimer.WaitTime = _lifetime;
        _lifetimeTimer.OneShot = true;
        _lifetimeTimer.Timeout += OnLifetimeTimeout;
        AddChild(_lifetimeTimer);
        _lifetimeTimer.Start();
    }

    private void SetupHitboxComponent()
    {
        if (HitboxComponent is null)
            return;

        HitboxComponent.AttackConnected += OnAttackHitboxHitConnected;
    }

    private void OnLifetimeTimeout()
    {
        BeforeLifetimeTimeout();
        QueueFree();
    }

    protected virtual void BeforeLifetimeTimeout()
    {
    }
}
