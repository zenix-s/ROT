using System;
using System.Collections.Generic;
using Godot;
using RotOfTime.Core.Entities;

namespace RotOfTime.Core.Combat.Components;

/// <summary>
///     Abstract generic attack manager that routes attack requests from the state machine
///     to AttackSlot instances. Each entity type provides its own TSlot enum defining
///     available attack slots. All lifecycle management (cooldown, cast timing, execution)
///     is delegated to the AttackSlot.
/// </summary>
public abstract partial class AttackManagerComponent<TSlot> : Node
    where TSlot : struct, Enum
{
    private readonly Dictionary<TSlot, AttackSlot> _slots = [];
    private readonly HashSet<TSlot> _activeExecutions = [];
    private readonly List<TSlot> _completedExecutions = [];

    private TSlot? _activeCastSlot;

    /// <summary>
    ///     Emitted when a cast starts. StringName is the TSlot.ToString().
    /// </summary>
    [Signal]
    public delegate void CastStartedEventHandler(StringName slot);

    /// <summary>
    ///     Emitted when a cast phase completes. StringName is the TSlot.ToString().
    /// </summary>
    [Signal]
    public delegate void CastCompletedEventHandler(StringName slot);

    /// <summary>
    ///     Emitted when an attack execution finishes entirely. StringName is the TSlot.ToString().
    /// </summary>
    [Signal]
    public delegate void AttackFinishedEventHandler(StringName slot);

    /// <summary>
    ///     Whether any non-instant cast is currently active.
    /// </summary>
    public bool IsCasting => _activeCastSlot.HasValue;

    /// <summary>
    ///     The currently active cast slot, if any.
    /// </summary>
    public TSlot? ActiveCastSlot => _activeCastSlot;

    /// <summary>
    ///     Whether movement is allowed during the current cast.
    /// </summary>
    public bool AllowMovementDuringCast
    {
        get
        {
            if (!_activeCastSlot.HasValue)
                return true;

            if (_slots.TryGetValue(_activeCastSlot.Value, out var slot))
                return slot.AllowMovementDuringCast;

            return true;
        }
    }

    protected void RegisterSlot(TSlot slotKey, AttackSlot slot)
    {
        if (_slots.ContainsKey(slotKey))
        {
            GD.PrintErr($"AttackManagerComponent: Slot '{slotKey}' already registered. Unregister first.");
            return;
        }

        _slots[slotKey] = slot;
    }

    protected void UnregisterSlot(TSlot slotKey)
    {
        _slots.Remove(slotKey);
        _activeExecutions.Remove(slotKey);

        if (_activeCastSlot.HasValue && EqualityComparer<TSlot>.Default.Equals(_activeCastSlot.Value, slotKey))
            _activeCastSlot = null;
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
        if (!_slots.TryGetValue(slotKey, out var slot))
            return false;

        if (!slot.IsReady)
            return false;

        // Block new non-instant casts if one is already active
        if (_activeCastSlot.HasValue && !slot.IsInstantCast)
            return false;

        slot.Activate(direction, position, stats, ownerNode);
        _activeExecutions.Add(slotKey);

        // Track active cast for non-instant abilities
        if (!slot.IsInstantCast)
            _activeCastSlot = slotKey;

        EmitSignal(SignalName.CastStarted, SlotToStringName(slotKey));

        // Instant cast completes immediately
        if (slot.IsInstantCast)
            EmitSignal(SignalName.CastCompleted, SlotToStringName(slotKey));

        return true;
    }

    /// <summary>
    ///     Check if a slot is on cooldown.
    /// </summary>
    public bool IsOnCooldown(TSlot slotKey)
    {
        return _slots.TryGetValue(slotKey, out var slot) && !slot.IsReady;
    }

    /// <summary>
    ///     Get the cooldown progress (0 = ready, 1 = just started) for a slot.
    /// </summary>
    public float GetCooldownProgress(TSlot slotKey)
    {
        if (!_slots.TryGetValue(slotKey, out var slot))
            return 0f;

        return slot.GetCooldownProgress();
    }

    /// <summary>
    ///     Get the AttackSlot assigned to a slot key.
    /// </summary>
    public AttackSlot GetSlot(TSlot slotKey)
    {
        return _slots.GetValueOrDefault(slotKey);
    }

    /// <summary>
    ///     Check if a slot has been registered.
    /// </summary>
    public bool HasSlot(TSlot slotKey)
    {
        return _slots.ContainsKey(slotKey);
    }

    public override void _Process(double delta)
    {
        _completedExecutions.Clear();

        // Tick all slots (cooldowns always tick, even when not executing)
        foreach (var (slotKey, slot) in _slots)
            slot.Process(delta);

        // Check active executions for cast completion and execution completion
        foreach (var slotKey in _activeExecutions)
        {
            var slot = _slots[slotKey];

            // Check if cast phase completed for the active cast slot
            if (_activeCastSlot.HasValue
                && EqualityComparer<TSlot>.Default.Equals(_activeCastSlot.Value, slotKey)
                && slot.IsCastComplete)
            {
                EmitSignal(SignalName.CastCompleted, SlotToStringName(slotKey));
                _activeCastSlot = null;
            }

            // Check if execution is fully complete
            if (slot.IsExecutionComplete)
                _completedExecutions.Add(slotKey);
        }

        // Remove completed executions and emit signals
        foreach (var slotKey in _completedExecutions)
        {
            _activeExecutions.Remove(slotKey);
            EmitSignal(SignalName.AttackFinished, SlotToStringName(slotKey));
        }
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
