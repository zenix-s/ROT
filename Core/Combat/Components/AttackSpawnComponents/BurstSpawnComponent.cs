using Godot;
using RotOfTime.Core.Combat.Attacks;
using RotOfTime.Scenes.Attacks.Projectiles;

namespace RotOfTime.Core.Combat.Components.AttackSpawnComponents;

[GlobalClass]
public partial class BurstSpawnComponent : AttackSpawnComponent
{
    [Export] public int BurstCount { get; set; } = 3;
    [Export] public float BurstDelay { get; set; } = 0.1f;

    private AttackContext _currentCtx;
    private int _fired;
    private Timer _burstTimer;

    public override void Execute(AttackContext ctx)
    {
        if (ProjectileScene == null || Data == null)
        {
            GD.PrintErr("BurstSpawnComponent: ProjectileScene or Data is null.");
            return;
        }

        _currentCtx = ctx;
        _fired = 0;

        SpawnOne();
        EmitSignal(SignalName.SkillFired, Data.CooldownDuration);

        if (BurstCount <= 1)
            return;

        _burstTimer = new Timer
        {
            WaitTime = BurstDelay,
            OneShot = false
        };
        _burstTimer.Timeout += OnBurstTick;
        AddChild(_burstTimer);
        _burstTimer.Start();
    }

    private void OnBurstTick()
    {
        SpawnOne();

        if (_fired >= BurstCount)
        {
            _burstTimer.Stop();
            _burstTimer.QueueFree();
            _burstTimer = null;
        }
    }

    private void SpawnOne()
    {
        var ctx = _currentCtx with { SpawnPosition = _currentCtx.Owner.GlobalPosition };

        var instance = ProjectileScene.Instantiate<Node2D>();
        instance.GlobalPosition = ctx.SpawnPosition;
        instance.Rotation = ctx.Direction.Angle();

        if (Data is ProjectileData projData)
        {
            var movement = MovementFactory.Create(projData.MovementType);
            instance.AddChild(movement);
        }

        ctx.AttacksContainer.AddChild(instance);

        if (instance is Projectile projectile)
        {
            projectile.Initialize(ctx, Data);
        }
        else
        {
            foreach (var child in instance.GetChildren())
            {
                if (child is AttackHitboxComponent hitbox)
                {
                    hitbox.Initialize(ctx.OwnerStats, Data, ctx.DamageMultiplier);
                    break;
                }
            }
        }

        _fired++;
    }
}
