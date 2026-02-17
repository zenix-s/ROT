using Godot;
using RotOfTime.Core.Combat.Attacks;
using RotOfTime.Core.Combat.Components;
using RotOfTime.Core.Entities;
using AttackHitboxComponent = RotOfTime.Core.Combat.Components.AttackHitboxComponent;
using AttackMovementComponent = RotOfTime.Core.Combat.Components.AttackMovementComponents.AttackMovementComponent;

namespace RotOfTime.Scenes.Attacks.Projectiles;

/// <summary>
///     Base projectile that moves through the scene and damages on contact.
///     Has no knowledge of its own stats until Execute() injects them.
///     The node is already positioned in the scene tree when Execute() is called.
/// </summary>
public partial class Projectile : CharacterBody2D, IAttack
{
    private Timer _lifetimeTimer;
    protected Vector2 Direction;

    [Export] public AttackMovementComponent MovementComponent { get; set; }
    [Export] public AttackHitboxComponent HitboxComponent { get; set; }

    public override void _Ready()
    {
        if (HitboxComponent is null || MovementComponent is null)
        {
            GD.PrintErr($"Projectile: Missing components on {Name}");
            QueueFree();
            return;
        }

        // Default direction from rotation (set by spawner before AddChild)
        Direction = Vector2.Right.Rotated(GlobalRotation);

        SetupLifetimeTimer();
        SetupHitboxComponent();
    }

    public void Execute(Vector2 direction, EntityStats ownerStats, AttackData attackData,
        Node2D owner, float damageMultiplier = 1.0f)
    {
        Direction = direction.Normalized();

        // Initialize movement from ProjectileData (node is in tree, components are ready)
        if (attackData is ProjectileData projectileData)
        {
            MovementComponent.Initialize(projectileData);
            _lifetimeTimer.WaitTime = projectileData.Lifetime;
            _lifetimeTimer.Start();
        }

        // Initialize damage on the hitbox
        HitboxComponent?.Initialize(ownerStats, attackData, damageMultiplier);
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
        _lifetimeTimer.WaitTime = 5;
        _lifetimeTimer.OneShot = true;
        _lifetimeTimer.Timeout += OnLifetimeTimeout;
        AddChild(_lifetimeTimer);
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
