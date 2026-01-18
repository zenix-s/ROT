using System.Collections.Generic;
using Godot;
using RotOfTime.Core.Combat.Attacks;
using RotOfTime.Core.Combat.Calculations;
using RotOfTime.Core.Entities;

namespace RotOfTime.Core.Components;

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
    [Export] public AttackHitboxComponent Hitbox { get; set; }
    [Export] public AttackType AttackType { get; set; }
    [Export] public GameConstants.Faction Faction { get; set; }

    public AttackResult CurrentAttackResult { get; private set; } = AttackResult.None;
    public AttackData AttackData => AttackRegistry.GetAttackData(AttackType);

    public override void _Ready()
    {
        if (Hitbox == null)
        {
            GD.PrintErr($"AttackDamageComponent: Hitbox not assigned on {GetParent()?.Name}");
            return;
        }

        Hitbox.CollisionLayer = (uint)AttackLayerMap[Faction];
        Hitbox.DamageComponent = this;
    }

    /// <summary>
    ///     Update the attack result based on owner's stats.
    ///     Call this when the attack is activated or owner stats change.
    /// </summary>
    public void UpdateStats(EntityStats ownerStats)
    {
        AttackData attackData = AttackRegistry.GetAttackData(AttackType);
        CurrentAttackResult = DamageCalculator.CalculateRawDamage(ownerStats, attackData);
        if (Hitbox != null)
            Hitbox.AttackResult = CurrentAttackResult;
    }

}
