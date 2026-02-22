using System.Collections.Generic;
using Godot;
using RotOfTime.Core.Combat.Attacks;
using RotOfTime.Core.Combat.Calculations;
using RotOfTime.Core.Combat.Results;
using RotOfTime.Core.Entities;

namespace RotOfTime.Core.Combat.Components;

[GlobalClass]
public partial class AttackHitboxComponent : Area2D
{
    private static readonly Dictionary<GameConstants.Faction, GameConstants.GameLayers> AttackLayerMap = new()
    {
        { GameConstants.Faction.Player, GameConstants.GameLayers.PlayerAttack },
        { GameConstants.Faction.Ally, GameConstants.GameLayers.PlayerAttack },
        { GameConstants.Faction.Enemy, GameConstants.GameLayers.EnemyAttack }
    };

    private static readonly Dictionary<GameConstants.Faction, GameConstants.GameLayers> TargetMaskMap = new()
    {
        { GameConstants.Faction.Player, GameConstants.GameLayers.EnemyDamageBox },
        { GameConstants.Faction.Ally, GameConstants.GameLayers.EnemyDamageBox },
        { GameConstants.Faction.Enemy, GameConstants.GameLayers.PlayerDamageBox }
    };

    private bool _factionInitialized;

    [Signal]
    public delegate void AttackConnectedEventHandler();
    
    [Signal]
    public delegate void WallHitEventHandler();

    [Export] public GameConstants.Faction Faction { get; set; }

    public AttackResult AttackResult { get; private set; } = AttackResult.None;

    public override void _Ready()
    {
        AreaEntered += OnAreaEntered;
        BodyEntered += OnTileMapCollision;

        if (!_factionInitialized)
            ApplyFaction(Faction);
            
    }

    public void Initialize(EntityStats ownerStats, AttackData attackData, float damageMultiplier = 1.0f)
    {
        ApplyFaction(ownerStats.Faction);
        AttackResult = CalculateDamage(ownerStats, attackData, damageMultiplier);
    }

    private AttackResult CalculateDamage(EntityStats ownerStats, AttackData attackData, float damageMultiplier)
    {
        if (attackData == null)
            return AttackResult.None;

        var baseResult = DamageCalculator.CalculateRawDamage(ownerStats, attackData);

        if (Mathf.IsEqualApprox(damageMultiplier, 1.0f))
            return baseResult;

        int modifiedDamage = Mathf.Max(1, (int)(baseResult.RawDamage * damageMultiplier));
        return new AttackResult(modifiedDamage, baseResult.AttackName, baseResult.IsCritical);
    }

    private void ApplyFaction(GameConstants.Faction faction)
    {
        Faction = faction;
        CollisionLayer = (uint)AttackLayerMap[faction];
        CollisionMask = (uint)TargetMaskMap[faction] | (uint)GameConstants.GameLayers.World;
        _factionInitialized = true;
    }

    private void OnTileMapCollision(Node2D node)
    {
        if (!node.IsInGroup(Groups.Walls)) return;

        EmitSignal(SignalName.WallHit);
    }

    private void OnAreaEntered(Area2D area)
    {
        if (area is not HurtboxComponent) return;
        EmitSignal(SignalName.AttackConnected);
    }

    public override void _ExitTree()
    {
        AreaEntered -= OnAreaEntered;
    }
}
