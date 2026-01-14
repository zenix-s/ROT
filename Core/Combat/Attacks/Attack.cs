using System.Collections.Generic;
using Godot;
using RotOfTime.Core.Combat.Calculations;
using RotOfTime.Core.Components.Hurtbox;
using RotOfTime.Core.Entities;
using RotOfTime.Core.Grimoire;

namespace RotOfTime.Core.Combat.Attacks;

/// <summary>
///     Unified attack area that transmits damage to hurtboxes.
///     Configurable behavior through properties:
///     - HitCooldown: Per-target cooldown (0 = one-shot per target)
///     - IsPermanent: Ignores lifetime, stays forever
///     - Speed/Direction: Movement (0 = stationary)
///     - Lifetime: Auto-destroy after duration (0 = infinite if permanent)
///     - DestroyOnHit: Remove attack on first hit
/// </summary>
public partial class Attack : Area2D, IAttack
{
    private float _lifetimeTimer;

    [Export] public float HitCooldown { get; set; }
    [Export] public bool IsPermanent { get; set; }
    [Export] public float Speed { get; set; }
    [Export] public Vector2 Direction { get; set; } = Vector2.Zero;
    [Export] public float Lifetime { get; set; }
    [Export] public bool DestroyOnHit { get; set; }
    [Export] public AttackData AttackData { get; set; }
    public AttackTag[] Tags { get; set; } = [AttackTag.Normal];

    public AttackResult AttackResult { get; private set; }

    public void UpdateStats(EntityStats entity, GrimoireStats grimoire)
    {
        AttackResult = AttackCalculator.Calculate(entity, grimoire, AttackData);
    }

    public override void _Ready()
    {
        Monitoring = true;
        Monitorable = true;

        _lifetimeTimer = Lifetime;
        AttackResult = AttackCalculator.CalculateBase(AttackData);

        AreaEntered += OnAttackAreaEntered;
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;

        // Movement
        if (Speed > 0 && Direction != Vector2.Zero)
            Position += Direction.Normalized() * Speed * dt;

        // Lifetime (skip if permanent)
        if (!IsPermanent && Lifetime > 0)
        {
            _lifetimeTimer -= dt;
            if (_lifetimeTimer <= 0)
                QueueFree();
        }
    }

    private void OnAttackAreaEntered(Area2D area)
    {
        if (area is not HurtboxComponent) return;

        if (DestroyOnHit)
            QueueFree();
    }
}
