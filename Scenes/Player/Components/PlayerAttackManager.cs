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
    public AttackSlot Ability1Slot { get; set; }
    public AttackSlot Ability2Slot { get; set; }
    public AttackSlot Ability3Slot { get; set; }

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

        if (Ability1Slot != null)
            RegisterSlot(PlayerAttackSlot.Ability1, Ability1Slot);

        if (Ability2Slot != null)
            RegisterSlot(PlayerAttackSlot.Ability2, Ability2Slot);

        if (Ability3Slot != null)
            RegisterSlot(PlayerAttackSlot.Ability3, Ability3Slot);
    }
}
