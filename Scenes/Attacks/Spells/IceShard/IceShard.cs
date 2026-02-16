using Godot;
using RotOfTime.Core.Combat.Attacks;
using RotOfTime.Core.Entities;

namespace RotOfTime.Scenes.Attacks.Spells.IceShard;

/// <summary>
///     Burst spell that rapidly fires 3 projectiles in sequence.
///     Implements IAttack but is not a projectile itself — it spawns sub-projectiles.
///     Self-destructs after all projectiles are spawned.
/// </summary>
public partial class IceShard : Node2D, IAttack
{
    /// <summary>
    ///     The projectile scene to instantiate for each burst shot.
    /// </summary>
    [Export] public PackedScene ProjectileScene { get; set; }

    /// <summary>
    ///     Data for each sub-projectile (speed, lifetime, etc.).
    ///     DamageCoefficient from the parent AttackData is used for damage calculation.
    /// </summary>
    [Export] public ProjectileData SubProjectileData { get; set; }

    /// <summary>
    ///     Number of projectiles in the burst.
    /// </summary>
    [Export] public int BurstCount { get; set; } = 3;

    /// <summary>
    ///     Delay in seconds between each projectile in the burst.
    /// </summary>
    [Export] public float BurstDelay { get; set; } = 0.1f;

    private Vector2 _direction;
    private EntityStats _ownerStats;
    private AttackData _attackData;
    private float _damageMultiplier;
    private int _projectilesFired;
    private Timer _burstTimer;

    public void Execute(Vector2 direction, EntityStats ownerStats, AttackData attackData,
        float damageMultiplier = 1.0f)
    {
        _direction = direction.Normalized();
        _ownerStats = ownerStats;
        _attackData = attackData;
        _damageMultiplier = damageMultiplier;

        // Fire first projectile immediately
        SpawnSubProjectile();

        if (BurstCount <= 1)
        {
            QueueFree();
            return;
        }

        // Set up timer for remaining projectiles
        _burstTimer = new Timer
        {
            WaitTime = BurstDelay,
            OneShot = false
        };
        _burstTimer.Timeout += OnBurstTimerTimeout;
        AddChild(_burstTimer);
        _burstTimer.Start();
    }

    private void OnBurstTimerTimeout()
    {
        SpawnSubProjectile();

        if (_projectilesFired >= BurstCount)
        {
            _burstTimer.Stop();
            QueueFree();
        }
    }

    private void SpawnSubProjectile()
    {
        if (ProjectileScene == null || SubProjectileData == null)
        {
            GD.PrintErr("IceShard: ProjectileScene or SubProjectileData is null.");
            QueueFree();
            return;
        }

        var attackContainer = GetTree().Root.GetNodeOrNull<Node2D>("Main/Attacks");
        if (attackContainer == null)
        {
            GD.PrintErr("IceShard: 'Main/Attacks' container not found.");
            QueueFree();
            return;
        }

        var projectile = ProjectileScene.Instantiate<Node2D>();
        projectile.GlobalPosition = GlobalPosition;
        projectile.Rotation = _direction.Angle();
        attackContainer.AddChild(projectile);

        // Use SubProjectileData for movement, but parent AttackData's DamageCoefficient for damage
        if (projectile is IAttack iAttack)
            iAttack.Execute(_direction, _ownerStats, SubProjectileData, _damageMultiplier);

        _projectilesFired++;
    }
}
