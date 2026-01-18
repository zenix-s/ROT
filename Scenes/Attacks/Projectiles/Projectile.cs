using Godot;
using RotOfTime.Core.Combat.Projectiles;
using RotOfTime.Core.Components;
using RotOfTime.Core.Entities;

public partial class Projectile : CharacterBody2D, IAttack
{
    protected Vector2 _direction;
    private bool _isLaunched;
    private Timer _lifetimeTimer;

    private int _speed;
    private int _initialSpeed = 200;
    private int _targetSpeed = 200;
    private double _acceleration;
    private int _lifetime = 5;

    [Export] public AttackDamageComponent DamageComponent { get; set; }

    public void ApplySettings(ProjectileSettings settings)
    {
        if (settings == null) return;
        _initialSpeed = settings.InitialSpeed;
        _targetSpeed = settings.TargetSpeed;
        _acceleration = settings.Acceleration;
        _lifetime = settings.Lifetime;
    }

    public void UpdateStats(EntityStats ownerStats)
    {
        DamageComponent?.UpdateStats(ownerStats);
    }

    public override void _Ready()
    {
        _speed = _initialSpeed;
        if (!_isLaunched)
            _direction = Vector2.Right.Rotated(GlobalRotation);

        SetupLifetimeTimer();
    }

    public override void _PhysicsProcess(double delta)
    {
        _speed = (int)Mathf.MoveToward(_speed, _targetSpeed, (float)(_acceleration * delta));
        Velocity = _direction * _speed;
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
        _direction = direction.Normalized();
        Rotation = _direction.Angle();
        UpdateStats(ownerStats);
        _isLaunched = true;
    }
}
