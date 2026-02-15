using System;
using Godot;
using RotOfTime.Autoload;
using RotOfTime.Core.Combat.Components;

namespace RotOfTime.Scenes.Player.Components;

/// <summary>
///     Player-specific attack manager. Maps PlayerAttackSlot enum values
///     to AttackSlot children discovered in the scene tree.
///     Each AttackSlot encapsulates its spawner, cooldown, and cast configuration.
/// </summary>
[GlobalClass]
public partial class PlayerAttackManager : AttackManagerComponent<PlayerAttackSlot>
{
    public AttackSlot BasicAttackSlot { get; set; }
    public AttackSlot Spell1Slot { get; set; }
    public AttackSlot Spell2Slot { get; set; }

    public override void _Ready()
    {
        RegisterExportedSlots();
        LoadPlayerAttacks();
    }

    private void LoadPlayerAttacks()
    {
        // GameManager.Instance.AbilityManager.LoadPlayerAttack(PlayerAttackSlot.BasicAttack, BasicAttackSlot);
    }

    private void RegisterExportedSlots()
    {
        if (BasicAttackSlot != null)
            RegisterSlot(PlayerAttackSlot.BasicAttack, BasicAttackSlot);

        if (Spell1Slot != null)
            RegisterSlot(PlayerAttackSlot.Spell1, Spell1Slot);

        if (Spell2Slot != null)
            RegisterSlot(PlayerAttackSlot.Spell2, Spell2Slot);
    }
}
