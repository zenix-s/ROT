using Godot;
using RotOfTime.Core.Combat.Attacks;
using RotOfTime.Core.Combat.Components;
using RotOfTime.Core.Combat.Components.AttackMovementComponents;

namespace RotOfTime.Scenes.Attacks.Projectiles;

public partial class Projectile : Area2D
{
    [Export] public AttackHitboxComponent HitboxComponent { get; set; }

    public void Initialize(AttackContext ctx, AttackData data)
    {
        HitboxComponent?.Initialize(ctx.OwnerStats, data, ctx.DamageMultiplier);

        if (data is ProjectileData projData)
        {
            var movement = FindMovementComponent();
            movement?.Initialize(projData, ctx.Direction);

            var timer = new Timer
            {
                WaitTime = projData.Lifetime,
                OneShot = true
            };
            timer.Timeout += OnLifetimeExpired;
            AddChild(timer);
            timer.Start();
        }

        if (HitboxComponent != null)
        {
            HitboxComponent.AttackConnected += OnImpact;
            HitboxComponent.WallHit += OnImpact;
        }
    }

    protected virtual void OnImpact()
    {
        QueueFree();
    }

    protected virtual void OnLifetimeExpired()
    {
        QueueFree();
    }

    private AttackMovementComponent FindMovementComponent()
    {
        foreach (var child in GetChildren())
        {
            if (child is AttackMovementComponent movement)
                return movement;
        }
        return null;
    }
}
