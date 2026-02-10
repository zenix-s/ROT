using System;
using System.Collections.Generic;
using Godot;
using RotOfTime.Core.Combat.Components.AttackSpawnerComponents;
using RotOfTime.Core.Entities;

namespace RotOfTime.Core.Combat.Components;

/// <summary>
///     Abstract generic attack manager that orchestrates attack execution, cooldowns, and casting.
///     Each entity type provides its own TSlot enum defining available attack slots.
///     Concrete implementations discover/register their spawner components.
/// </summary>
public abstract partial class AttackManagerComponent<TSlot> : Node
    where TSlot : struct, Enum
{
    private readonly Dictionary<TSlot, AttackSpawnerComponent> _slots = [];
    private readonly Dictionary<TSlot, float> _cooldowns = [];
    private readonly Dictionary<TSlot, ActiveExecution> _activeExecutions = [];
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

            if (_slots.TryGetValue(_activeCastSlot.Value, out var spawner))
                return spawner.AllowMovementDuringCast;

            return true;
        }
    }

    /// <summary>
    ///     Register an attack spawner for a slot.
    /// </summary>
    protected void RegisterSlot(TSlot slot, AttackSpawnerComponent spawner)
    {
        if (_slots.ContainsKey(slot))
        {
            GD.PrintErr($"AttackManagerComponent: Slot '{slot}' already registered. Unregister first.");
            return;
        }

        _slots[slot] = spawner;
        _cooldowns[slot] = 0f;
    }

    /// <summary>
    ///     Unregister an attack spawner from a slot.
    /// </summary>
    protected void UnregisterSlot(TSlot slot)
    {
        _slots.Remove(slot);
        _cooldowns.Remove(slot);
        _activeExecutions.Remove(slot);

        if (_activeCastSlot.HasValue && EqualityComparer<TSlot>.Default.Equals(_activeCastSlot.Value, slot))
            _activeCastSlot = null;
    }

    /// <summary>
    ///     Attempt to fire an attack from the given slot.
    /// </summary>
    public bool TryFire(TSlot slot, Vector2 direction, Vector2 position, EntityStats stats)
    {
        if (!_slots.TryGetValue(slot, out var spawner))
            return false;

        // Check cooldown
        if (_cooldowns.TryGetValue(slot, out var cooldown) && cooldown > 0f)
            return false;

        // Block new casts if a casted ability is already active
        if (_activeCastSlot.HasValue && !spawner.IsInstantCast)
            return false;

        // Reset and activate the spawner
        spawner.Reset();
        spawner.Activate(direction, position, stats);

        // Track execution
        var execution = new ActiveExecution { ElapsedTime = 0f, CastCompleted = false };
        _activeExecutions[slot] = execution;
        _cooldowns[slot] = spawner.Cooldown;

        // Track active cast for non-instant abilities
        if (!spawner.IsInstantCast)
            _activeCastSlot = slot;

        EmitSignal(SignalName.CastStarted, SlotToStringName(slot));

        // Instant cast completes immediately
        if (spawner.IsInstantCast)
        {
            execution.CastCompleted = true;
            EmitSignal(SignalName.CastCompleted, SlotToStringName(slot));
        }

        return true;
    }

    /// <summary>
    ///     Check if a slot is on cooldown.
    /// </summary>
    public bool IsOnCooldown(TSlot slot)
    {
        return _cooldowns.TryGetValue(slot, out var cooldown) && cooldown > 0f;
    }

    /// <summary>
    ///     Get the cooldown progress (0 = ready, 1 = just started) for a slot.
    /// </summary>
    public float GetCooldownProgress(TSlot slot)
    {
        if (!_slots.TryGetValue(slot, out var spawner) || spawner.Cooldown <= 0f)
            return 0f;

        if (!_cooldowns.TryGetValue(slot, out var remaining))
            return 0f;

        return Mathf.Clamp(remaining / spawner.Cooldown, 0f, 1f);
    }

    /// <summary>
    ///     Get the spawner assigned to a slot.
    /// </summary>
    public AttackSpawnerComponent GetSpawner(TSlot slot)
    {
        return _slots.GetValueOrDefault(slot);
    }

    /// <summary>
    ///     Check if a slot has a spawner registered.
    /// </summary>
    public bool HasSlot(TSlot slot)
    {
        return _slots.ContainsKey(slot);
    }

    public override void _Process(double delta)
    {
        var dt = (float)delta;

        // Update cooldowns
        foreach (var slot in _cooldowns.Keys)
        {
            if (_cooldowns[slot] > 0f)
                _cooldowns[slot] -= dt;
        }

        // Process active executions
        _completedExecutions.Clear();

        foreach (var (slot, execution) in _activeExecutions)
        {
            if (!_slots.TryGetValue(slot, out var spawner))
            {
                _completedExecutions.Add(slot);
                continue;
            }

            execution.ElapsedTime += dt;

            // Let spawner handle its multi-shot logic
            spawner.Process(delta);

            // Check if cast phase is complete
            if (!execution.CastCompleted && execution.ElapsedTime >= spawner.CastDuration)
            {
                execution.CastCompleted = true;
                EmitSignal(SignalName.CastCompleted, SlotToStringName(slot));

                if (_activeCastSlot.HasValue &&
                    EqualityComparer<TSlot>.Default.Equals(_activeCastSlot.Value, slot))
                    _activeCastSlot = null;
            }

            // Check if execution is fully complete
            if (spawner.IsComplete)
                _completedExecutions.Add(slot);
        }

        // Remove completed executions
        foreach (var slot in _completedExecutions)
        {
            _activeExecutions.Remove(slot);
            EmitSignal(SignalName.AttackFinished, SlotToStringName(slot));
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

    private class ActiveExecution
    {
        public float ElapsedTime { get; set; }
        public bool CastCompleted { get; set; }
    }
}
