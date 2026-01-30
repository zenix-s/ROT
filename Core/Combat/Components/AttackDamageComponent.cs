using System.Collections.Generic;
using Godot;
using RotOfTime.Core.Combat.Attacks;
using RotOfTime.Core.Combat.Calculations;
using RotOfTime.Core.Combat.Results;
using RotOfTime.Core.Entities;

namespace RotOfTime.Core.Combat.Components;

/// <summary>
///     Component that centralizes damage calculation for attacks.
///     Attach this to any attack node alongside an AttackHitboxComponent.
/// </summary>
public partial class AttackDamageComponent : Node
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

    private bool _factionOverridden;

    [Export] public AttackHitboxComponent Hitbox { get; set; }
    [Export] public GameConstants.Faction Faction { get; set; }
    [Export] public AttackData AttackData { get; set; }

    public AttackResult CurrentAttackResult { get; private set; } = AttackResult.None;

    public override void _Ready()
    {
        if (Hitbox == null)
        {
            GD.PrintErr($"AttackDamageComponent: Hitbox not assigned on {GetParent()?.Name}");
            return;
        }

        if (!_factionOverridden)
            ApplyFaction(Faction);
    }

    /// <summary>
    ///     Update the attack result based on owner's stats.
    ///     Call this when the attack is activated or owner stats change.
    /// </summary>
    public void UpdateStats(EntityStats ownerStats)
    {
        ApplyFaction(ownerStats.Faction);
        CurrentAttackResult = DamageCalculator.CalculateRawDamage(ownerStats, AttackData);
        if (Hitbox != null)
            Hitbox.AttackResult = CurrentAttackResult;
    }

    private void ApplyFaction(GameConstants.Faction faction)
    {
        Faction = faction;
        if (Hitbox == null) return;
        Hitbox.CollisionLayer = (uint)AttackLayerMap[faction];
        Hitbox.CollisionMask = (uint)TargetMaskMap[faction];
        _factionOverridden = true;
    }
}
