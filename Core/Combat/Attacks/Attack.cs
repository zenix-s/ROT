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
    [Export] public AttackData AttackData { get; set; }
    public AttackTag[] Tags { get; set; } = [AttackTag.Normal];

    [Export] public float HitCooldown { get; set; } = 0f;
    [Export] public bool IsPermanent { get; set; } = false;
    [Export] public float Speed { get; set; } = 0f;
    [Export] public Vector2 Direction { get; set; } = Vector2.Zero;
    [Export] public float Lifetime { get; set; } = 0f;
    [Export] public bool DestroyOnHit { get; set; } = false;

    public AttackResult AttackResult { get; private set; }

    private readonly Dictionary<ulong, float> _targetCooldowns = new();
    private float _lifetimeTimer;

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
        var dt = (float)delta;

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

        // Update per-target cooldowns
        UpdateCooldowns(dt);
    }

    public void UpdateStats(EntityStats entity, GrimoireStats grimoire)
    {
        AttackResult = AttackCalculator.Calculate(entity, grimoire, AttackData);
    }

    public bool CanHit(ulong targetId)
    {
        if (HitCooldown <= 0)
            return !_targetCooldowns.ContainsKey(targetId);

        return !_targetCooldowns.ContainsKey(targetId) || _targetCooldowns[targetId] <= 0;
    }

    public void RegisterHit(ulong targetId)
    {
        if (HitCooldown > 0)
            _targetCooldowns[targetId] = HitCooldown;
        else
            _targetCooldowns[targetId] = float.MaxValue; // One-shot: never hit again
    }

    private void UpdateCooldowns(float delta)
    {
        if (HitCooldown <= 0) return;

        var expiredKeys = new List<ulong>();
        foreach (var kvp in _targetCooldowns)
        {
            _targetCooldowns[kvp.Key] -= delta;
            if (_targetCooldowns[kvp.Key] <= 0)
                expiredKeys.Add(kvp.Key);
        }
        foreach (var key in expiredKeys)
            _targetCooldowns.Remove(key);
    }

    private void OnAttackAreaEntered(Area2D area)
    {
        if (area is not HurtboxComponent) return;

        if (DestroyOnHit)
            QueueFree();
    }
}
