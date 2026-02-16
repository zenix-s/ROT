using System;
using Godot;
using RotOfTime.Autoload;
using RotOfTime.Core.Combat.Attacks;
using RotOfTime.Core.Combat.Components;

namespace RotOfTime.Scenes.Player.Components;

/// <summary>
///     Player-specific attack manager. Manages 3 attack slots:
///     BasicAttack (LMB), Spell1 (Key 1), Spell2 (Key 2).
///     Each slot has an AttackData resource and a Timer for cooldown tracking.
/// </summary>
[GlobalClass]
public partial class PlayerAttackManager : AttackManagerComponent<PlayerAttackSlot>
{
    [Export] public AttackData BasicAttackData { get; set; }
    [Export] public AttackData Spell1Data { get; set; }
    [Export] public AttackData Spell2Data { get; set; }

    public override void _Ready()
    {
        RegisterSlotsAndTimers();
    }

    private void RegisterSlotsAndTimers()
    {
        // Register BasicAttack
        if (BasicAttackData != null)
        {
            RegisterSlot(PlayerAttackSlot.BasicAttack, BasicAttackData);
            CreateTimerForSlot(PlayerAttackSlot.BasicAttack);
        }

        // Register Spell1
        if (Spell1Data != null)
        {
            RegisterSlot(PlayerAttackSlot.Spell1, Spell1Data);
            CreateTimerForSlot(PlayerAttackSlot.Spell1);
        }

        // Register Spell2
        if (Spell2Data != null)
        {
            RegisterSlot(PlayerAttackSlot.Spell2, Spell2Data);
            CreateTimerForSlot(PlayerAttackSlot.Spell2);
        }
    }

    private void CreateTimerForSlot(PlayerAttackSlot slot)
    {
        var timer = new Timer
        {
            OneShot = true,
            Name = $"{slot}Timer"
        };

        AddChild(timer);
        RegisterTimer(slot, timer);
    }
}
