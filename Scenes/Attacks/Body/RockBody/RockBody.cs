using System;
using Godot;
using RotOfTime.Core.Combat;
using RotOfTime.Core.Entities;
using AttackDamageComponent = RotOfTime.Core.Combat.Components.AttackDamageComponent;
using AttackHitboxComponent = RotOfTime.Core.Combat.Components.AttackHitboxComponent;

namespace RotOfTime.Scenes.Attacks.Body.RockBody;

/// <summary>
///     Body-based melee attack.
///     Simple attack that damages on contact, used for melee enemies.
/// </summary>
public partial class RockBody : Area2D, IAttack
{
    [Export] public AttackDamageComponent DamageComponent { get; set; }
    [Export] public AttackHitboxComponent HitboxComponent { get; set; }

    public event Action CastCompleted;
    public event Action AttackFinished;
    public float Cooldown => 2.0f;
    public bool IsInstantCast => true;
    public bool AllowMovementDuringCast => true;

    public void Execute(Vector2 direction, Vector2 position, EntityStats ownerStats)
    {
        DamageComponent?.UpdateStats(ownerStats);
        CastCompleted?.Invoke();
    }
}
