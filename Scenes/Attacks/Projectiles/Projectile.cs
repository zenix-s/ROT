using Godot;
using RotOfTime.Core.Combat.Data;
using RotOfTime.Core.Components;
using RotOfTime.Core.Entities;

namespace RotOfTime.Scenes.Attacks.Projectiles;

public partial class Projectile : CharacterBody2D, IAttack
{
    private double _acceleration;
    protected Vector2 Direction;
    private int _initialSpeed = 200;
    private bool _isLaunched;
    private int _lifetime = 5;
    private Timer _lifetimeTimer;

    private int _speed;
    private int _targetSpeed = 200;

    [Export] public AttackDamageComponent DamageComponent { get; set; }

    public void UpdateStats(EntityStats ownerStats)
    {
        DamageComponent?.UpdateStats(ownerStats);
    }

    public void ApplySettings(AttackData settings)
    {
        if (settings == null) return;
        _initialSpeed = settings.InitialSpeed;
        _targetSpeed = settings.TargetSpeed;
        _acceleration = settings.Acceleration;
        _lifetime = settings.Lifetime;
    }

    public override void _Ready()
    {
        _speed = _initialSpeed;
        if (!_isLaunched)
            Direction = Vector2.Right.Rotated(GlobalRotation);

        SetupLifetimeTimer();
    }

    public override void _PhysicsProcess(double delta)
    {
        _speed = (int)Mathf.MoveToward(_speed, _targetSpeed, (float)(_acceleration * delta));
        Velocity = Direction * _speed;
        bool collision = MoveAndSlide();
        if (collision)
            QueueFree();
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

    private void OnLifetimeTimeout()
    {
        BeforeLifetimeTimeout();
        QueueFree();
    }

    protected virtual void BeforeLifetimeTimeout()
    {
    }

    public void Launch(Vector2 fromPosition, Vector2 direction, EntityStats ownerStats)
    {
        GlobalPosition = fromPosition;
        Direction = direction.Normalized();
        Rotation = Direction.Angle();
        UpdateStats(ownerStats);
        _isLaunched = true;
    }
}
