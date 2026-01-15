using System;
using System.Collections.Generic;
using Godot;
using RotOfTime.Core.Components;
using RotOfTime.Core.Entities;

namespace RotOfTime.Core.Combat.Attacks;

public partial class Attack : Node2D, IAttack
{
    [Export] public AttackData AttackData { get; set; }
    [Export] public GameConstants.Faction Faction { get; set; }
    [Export] public AttackHitboxComponent AttackHitbox { get; set; }

    public AttackResult AttackResult { get; private set; }

    private Dictionary<GameConstants.Faction, GameConstants.GameLayers> AttackLayer => new()
    {
        { GameConstants.Faction.Player, GameConstants.GameLayers.PlayerAttack },
        { GameConstants.Faction.Ally, GameConstants.GameLayers.PlayerAttack },
        { GameConstants.Faction.Enemy, GameConstants.GameLayers.EnemyAttack }
    };

    public override void _Ready()
    {
        if (AttackHitbox == null)
            throw new InvalidOperationException("Hitbox Area2D not found");
        if (AttackData == null)
            throw new InvalidOperationException("AttackData is not set");
        AttackHitbox.CollisionLayer = (uint)AttackLayer[Faction];
    }

    public void UpdateStats(EntityStats entity)
    {
        AttackResult = AttackData.ToAttackResult(entity);
        AttackHitbox.AttackResult = AttackResult;
    }
}
