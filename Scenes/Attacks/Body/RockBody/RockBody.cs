using Godot;
using RotOfTime.Core.Combat.Attacks;
using RotOfTime.Core.Entities;
using AttackHitboxComponent = RotOfTime.Core.Combat.Components.AttackHitboxComponent;

namespace RotOfTime.Scenes.Attacks.Body.RockBody;

/// <summary>
///     Body-based melee attack.
///     Simple attack that damages on contact, used for melee enemies.
///     Has no knowledge of its own stats until Execute() injects them.
///     The node is already positioned in the scene tree when Execute() is called.
/// </summary>
public partial class RockBody : Area2D, IAttack
{
    [Export] public AttackHitboxComponent HitboxComponent { get; set; }

    public void Execute(Vector2 direction, EntityStats ownerStats, AttackData attackData,
        Node2D owner, float damageMultiplier = 1.0f)
    {
        HitboxComponent?.Initialize(ownerStats, attackData, damageMultiplier);
    }
}
