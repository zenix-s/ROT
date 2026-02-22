using System;
using System.Collections.Generic;
using Godot;
using RotOfTime.Core.Combat.Attacks;
using RotOfTime.Core.Combat.Skills;
using RotOfTime.Core.Entities;

namespace RotOfTime.Core.Combat.Components;

public abstract partial class AttackManagerComponent<TSlot> : Node
    where TSlot : struct, Enum
{
    private readonly Dictionary<TSlot, ActiveSkill> _skills = new();

    [Signal]
    public delegate void AttackFiredEventHandler(StringName slot);

    protected void RegisterSkill(TSlot slotKey, PackedScene skillScene)
    {
        if (skillScene == null)
        {
            GD.PrintErr($"AttackManagerComponent: Skill scene is null for slot '{slotKey}'.");
            return;
        }

        var skillInstance = skillScene.Instantiate<Node2D>();

        if (skillInstance is not ActiveSkill activeSkill)
        {
            GD.PrintErr($"AttackManagerComponent: Scene for slot '{slotKey}' is not an ActiveSkill.");
            skillInstance.QueueFree();
            return;
        }

        AddChild(activeSkill);
        _skills[slotKey] = activeSkill;
    }

    public bool TryFire(TSlot slotKey, Vector2 direction, Vector2 position, EntityStats stats, Node2D ownerNode)
    {
        if (!_skills.TryGetValue(slotKey, out var skill))
            return false;

        var attackContainer = GetTree().Root.GetNodeOrNull<Node>("Main/Attacks");
        if (attackContainer == null)
        {
            GD.PrintErr("AttackManagerComponent: 'Main/Attacks' container not found.");
            return false;
        }

        var ctx = new AttackContext(direction, position, stats, ownerNode, 1.0f, attackContainer);
        if (!skill.TryExecute(ctx))
            return false;

        EmitSignal(SignalName.AttackFired, SlotToStringName(slotKey));
        return true;
    }

    public bool IsOnCooldown(TSlot slotKey)
    {
        if (!_skills.TryGetValue(slotKey, out var skill))
            return false;
        return !skill.IsReady;
    }

    public float GetCooldownProgress(TSlot slotKey)
    {
        if (!_skills.TryGetValue(slotKey, out var skill))
            return 0f;
        return skill.GetCooldownProgress();
    }

    public bool HasSlot(TSlot slotKey)
    {
        return _skills.ContainsKey(slotKey);
    }

    private static StringName SlotToStringName(TSlot slot)
    {
        return slot.ToString();
    }

    public static bool TryParseSlot(StringName name, out TSlot slot)
    {
        return Enum.TryParse(name.ToString(), out slot);
    }
}
