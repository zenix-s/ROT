using System;
using System.Collections.Generic;
using Godot;
using RotOfTime.Core.Combat.Attacks;
using RotOfTime.Core.Entities;

namespace RotOfTime.Core.Combat.Components;

/// <summary>
///     Abstract generic attack manager that routes attack requests to attack scenes.
///     Uses Timer nodes for cooldown tracking (no manual cooldown logic).
///     All attacks are instant (no casting phase).
/// </summary>
public abstract partial class AttackManagerComponent<TSlot> : Node
    where TSlot : struct, Enum
{
    private readonly Dictionary<TSlot, AttackData> _slotData = new();
    private readonly Dictionary<TSlot, Timer> _slotTimers = new();

    /// <summary>
    ///     Emitted when an attack is fired. StringName is the TSlot.ToString().
    /// </summary>
    [Signal]
    public delegate void AttackFiredEventHandler(StringName slot);

    protected void RegisterSlot(TSlot slotKey, AttackData data)
    {
        if (_slotData.ContainsKey(slotKey))
        {
            GD.PrintErr($"AttackManagerComponent: Slot '{slotKey}' already registered. Unregister first.");
            return;
        }

        _slotData[slotKey] = data;
    }

    protected void RegisterTimer(TSlot slotKey, Timer timer)
    {
        if (_slotTimers.ContainsKey(slotKey))
        {
            GD.PrintErr($"AttackManagerComponent: Timer for slot '{slotKey}' already registered.");
            return;
        }

        _slotTimers[slotKey] = timer;
    }

    protected void UnregisterSlot(TSlot slotKey)
    {
        _slotData.Remove(slotKey);
        _slotTimers.Remove(slotKey);
    }

    /// <summary>
    ///     Attempt to fire an attack from the given slot.
    /// </summary>
    /// <param name="slotKey">Which attack slot to fire</param>
    /// <param name="direction">Normalized aim direction</param>
    /// <param name="position">World spawn position</param>
    /// <param name="stats">Owner's entity stats</param>
    /// <param name="ownerNode">The entity node that owns this attack (for position tracking)</param>
    public bool TryFire(TSlot slotKey, Vector2 direction, Vector2 position, EntityStats stats, Node2D ownerNode)
    {
        if (!_slotData.TryGetValue(slotKey, out var data))
            return false;

        if (!_slotTimers.TryGetValue(slotKey, out var timer))
            return false;

        if (!timer.IsStopped())
            return false; // On cooldown

        SpawnAttack(data, direction, position, stats);
        timer.Start(data.CooldownDuration);

        EmitSignal(SignalName.AttackFired, SlotToStringName(slotKey));
        return true;
    }

    private void SpawnAttack(AttackData data, Vector2 direction, Vector2 position, EntityStats stats)
    {
        if (data?.AttackScene == null)
        {
            GD.PrintErr($"AttackManagerComponent: AttackData or AttackScene is null.");
            return;
        }

        var attack = data.AttackScene.Instantiate<Node2D>();
        attack.GlobalPosition = position;
        attack.Rotation = direction.Angle();

        // Find or create attack container
        var root = GetTree().Root;
        var attackContainer = root.GetNodeOrNull<Node2D>("Main/Attacks");

        if (attackContainer == null)
        {
            GD.PrintErr("AttackManagerComponent: 'Main/Attacks' container not found. Cannot spawn attack.");
            attack.QueueFree();
            return;
        }

        attackContainer.AddChild(attack);

        // Execute attack if it implements IAttack
        if (attack is IAttack iAttack)
            iAttack.Execute(direction, stats, data);
    }

    /// <summary>
    ///     Check if a slot is on cooldown.
    /// </summary>
    public bool IsOnCooldown(TSlot slotKey)
    {
        if (!_slotTimers.TryGetValue(slotKey, out var timer))
            return false;

        return !timer.IsStopped();
    }

    /// <summary>
    ///     Get the cooldown progress (0 = ready, 1 = just started) for a slot.
    /// </summary>
    public float GetCooldownProgress(TSlot slotKey)
    {
        if (!_slotTimers.TryGetValue(slotKey, out var timer))
            return 0f;

        if (timer.IsStopped())
            return 0f;

        return Mathf.Clamp((float)(timer.TimeLeft / timer.WaitTime), 0f, 1f);
    }

    /// <summary>
    ///     Get the AttackData assigned to a slot key.
    /// </summary>
    public AttackData GetSlotData(TSlot slotKey)
    {
        return _slotData.GetValueOrDefault(slotKey);
    }

    /// <summary>
    ///     Check if a slot has been registered.
    /// </summary>
    public bool HasSlot(TSlot slotKey)
    {
        return _slotData.ContainsKey(slotKey);
    }

    /// <summary>
    ///     Convert a TSlot enum value to a StringName for signal emission.
    /// </summary>
    private static StringName SlotToStringName(TSlot slot)
    {
        return slot.ToString();
    }

    /// <summary>
    ///     Try to parse a StringName back to a TSlot enum value.
    /// </summary>
    public static bool TryParseSlot(StringName name, out TSlot slot)
    {
        return Enum.TryParse(name.ToString(), out slot);
    }
}
