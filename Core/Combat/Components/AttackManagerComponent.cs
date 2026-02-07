using System.Collections.Generic;
using Godot;
using RotOfTime.Core.Entities;

namespace RotOfTime.Core.Combat.Components;

[GlobalClass]
public partial class AttackManagerComponent : Node
{
    private readonly Dictionary<StringName, AttackSlotEntry> _attacks = new();

    [Signal]
    public delegate void CastStartedEventHandler(StringName attackKey);

    [Signal]
    public delegate void CastCompletedEventHandler(StringName attackKey);

    [Signal]
    public delegate void AttackFinishedEventHandler(StringName attackKey);

    public void RegisterAttack(StringName key, PackedScene scene)
    {
        if (_attacks.ContainsKey(key))
        {
            GD.PrintErr($"AttackManagerComponent: Key '{key}' already registered. Unregister first.");
            return;
        }

        // Temp instance to read metadata
        var tempNode = scene.Instantiate<Node2D>();
        if (tempNode is not IAttack attack)
        {
            GD.PrintErr($"AttackManagerComponent: Scene for key '{key}' does not implement IAttack.");
            tempNode.QueueFree();
            return;
        }

        var entry = new AttackSlotEntry
        {
            Scene = scene,
            Cooldown = attack.Cooldown,
            CooldownRemaining = 0f,
            IsInstantCast = attack.IsInstantCast,
            AllowMovementDuringCast = attack.AllowMovementDuringCast
        };

        _attacks[key] = entry;
        tempNode.QueueFree();
    }

    public void UnregisterAttack(StringName key)
    {
        _attacks.Remove(key);
    }

    public IAttack TryFire(StringName key, Vector2 direction, Vector2 position, EntityStats stats)
    {
        if (!_attacks.TryGetValue(key, out var entry))
            return null;

        if (entry.CooldownRemaining > 0f)
            return null;

        var node = entry.Scene.Instantiate<Node2D>();
        if (node is not IAttack attack)
        {
            node.QueueFree();
            return null;
        }

        attack.Execute(direction, position, stats);
        GetTree().CurrentScene.AddChild(node);

        entry.CooldownRemaining = entry.Cooldown;

        EmitSignal(SignalName.CastStarted, key);

        // Subscribe to IAttack events to re-emit as signals
        StringName capturedKey = key;

        if (attack.IsInstantCast)
        {
            EmitSignal(SignalName.CastCompleted, capturedKey);
        }
        else
        {
            attack.CastCompleted += () => EmitSignal(SignalName.CastCompleted, capturedKey);
        }

        attack.AttackFinished += () => EmitSignal(SignalName.AttackFinished, capturedKey);

        return attack;
    }

    public bool IsOnCooldown(StringName key)
    {
        return _attacks.TryGetValue(key, out var entry) && entry.CooldownRemaining > 0f;
    }

    public float GetCooldownProgress(StringName key)
    {
        if (!_attacks.TryGetValue(key, out var entry) || entry.Cooldown <= 0f)
            return 0f;

        return Mathf.Clamp(entry.CooldownRemaining / entry.Cooldown, 0f, 1f);
    }

    public AttackSlotEntry GetAttackMetadata(StringName key)
    {
        return _attacks.GetValueOrDefault(key);
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;
        foreach (var entry in _attacks.Values)
        {
            if (entry.CooldownRemaining > 0f)
                entry.CooldownRemaining -= dt;
        }
    }

    public class AttackSlotEntry
    {
        public PackedScene Scene { get; set; }
        public float Cooldown { get; set; }
        public float CooldownRemaining { get; set; }
        public bool IsInstantCast { get; set; }
        public bool AllowMovementDuringCast { get; set; }
    }
}
