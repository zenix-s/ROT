using System;
using System.Collections.Generic;
using Godot;
using RotOfTime.Core.Combat.Attacks;
using RotOfTime.Core.Combat.Components.AttackSpawnComponents;
using RotOfTime.Core.Entities;

namespace RotOfTime.Core.Combat.Components;

public abstract partial class AttackManagerComponent<TSlot> : Node
    where TSlot : struct, Enum
{
    private readonly Dictionary<TSlot, AttackSpawnComponent> _slotSkills = new();
    private readonly Dictionary<TSlot, Timer> _slotTimers = new();

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
        AddChild(skillInstance);

        AttackSpawnComponent spawnComponent = null;
        foreach (var child in skillInstance.GetChildren())
        {
            if (child is AttackSpawnComponent spawn)
            {
                spawnComponent = spawn;
                break;
            }
        }

        if (spawnComponent == null)
        {
            GD.PrintErr($"AttackManagerComponent: No AttackSpawnComponent found in skill for slot '{slotKey}'.");
            skillInstance.QueueFree();
            return;
        }

        _slotSkills[slotKey] = spawnComponent;

        var timer = new Timer
        {
            OneShot = true,
            Name = $"{slotKey}Timer"
        };
        AddChild(timer);
        _slotTimers[slotKey] = timer;

        spawnComponent.SkillFired += (cooldown) => OnSkillFired(slotKey, cooldown);
    }

    private void OnSkillFired(TSlot slotKey, float cooldown)
    {
        if (_slotTimers.TryGetValue(slotKey, out var timer))
            timer.Start(cooldown);

        EmitSignal(SignalName.AttackFired, SlotToStringName(slotKey));
    }

    public bool TryFire(TSlot slotKey, Vector2 direction, Vector2 position, EntityStats stats, Node2D ownerNode)
    {
        if (!_slotSkills.TryGetValue(slotKey, out var spawnComponent))
            return false;

        if (!_slotTimers.TryGetValue(slotKey, out var timer))
            return false;

        if (!timer.IsStopped())
            return false;

        var attackContainer = GetTree().Root.GetNodeOrNull<Node>("Main/Attacks");
        if (attackContainer == null)
        {
            GD.PrintErr("AttackManagerComponent: 'Main/Attacks' container not found.");
            return false;
        }

        var ctx = new AttackContext(direction, position, stats, ownerNode, 1.0f, attackContainer);
        spawnComponent.Execute(ctx);
        return true;
    }

    public bool IsOnCooldown(TSlot slotKey)
    {
        if (!_slotTimers.TryGetValue(slotKey, out var timer))
            return false;
        return !timer.IsStopped();
    }

    public float GetCooldownProgress(TSlot slotKey)
    {
        if (!_slotTimers.TryGetValue(slotKey, out var timer))
            return 0f;
        if (timer.IsStopped())
            return 0f;
        return Mathf.Clamp((float)(timer.TimeLeft / timer.WaitTime), 0f, 1f);
    }

    public bool HasSlot(TSlot slotKey)
    {
        return _slotSkills.ContainsKey(slotKey);
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
