using Godot;
using RotOfTime.Core.Combat.Attacks;
using RotOfTime.Core.Entities;

namespace RotOfTime.Core.Combat.Components.AttackSpawnerComponents;

/// <summary>
///     Abstract base for attack spawner components.
///     Each spawner is a reusable Node in the scene tree that knows what to spawn,
///     how to spawn it, and its cast/cooldown configuration.
///     Replaces the old SpawnerStrategy + AttackDefinition pattern.
/// </summary>
public abstract partial class AttackSpawnerComponent : Node
{
    /// <summary>
    ///     The attack scene to instantiate (e.g., a Projectile scene).
    /// </summary>
    [Export]
    public PackedScene AttackScene { get; set; }

    /// <summary>
    ///     Duration of the cast phase in seconds. 0 = instant cast.
    /// </summary>
    [Export]
    public float CastDuration { get; set; }

    /// <summary>
    ///     Cooldown duration in seconds before this attack can be used again.
    /// </summary>
    [Export]
    public float Cooldown { get; set; }

    /// <summary>
    ///     If true, the entity can move during the cast phase.
    ///     Only relevant for non-instant casts.
    /// </summary>
    [Export]
    public bool AllowMovementDuringCast { get; set; } = true;

    /// <summary>
    ///     Whether this is an instant cast (CastDuration == 0).
    /// </summary>
    public bool IsInstantCast => CastDuration <= 0f;

    /// <summary>
    ///     Whether this spawner has finished spawning all attacks for the current activation.
    /// </summary>
    public abstract bool IsComplete { get; }

    /// <summary>
    ///     Called once when the attack is fired. Stores context and may spawn immediately.
    /// </summary>
    public abstract void Activate(Vector2 direction, Vector2 position, EntityStats ownerStats);

    /// <summary>
    ///     Called every frame while the spawner is active (for multi-shot patterns).
    /// </summary>
    public abstract void Process(double delta);

    /// <summary>
    ///     Reset state for reuse. Spawners live in the scene tree and are reused between activations.
    /// </summary>
    public abstract void Reset();

    /// <summary>
    ///     Shared helper to instantiate an attack scene, execute it, and add it to the scene tree.
    /// </summary>
    protected void SpawnAttackInstance(Vector2 direction, Vector2 position, EntityStats ownerStats)
    {
        if (AttackScene == null)
        {
            GD.PrintErr($"AttackSpawnerComponent: AttackScene not assigned on {Name}");
            return;
        }

        var node = AttackScene.Instantiate<Node2D>();

        if (node is IAttack attack)
        {
            attack.Execute(direction, position, ownerStats);
        }
        else
        {
            GD.PrintErr($"AttackSpawnerComponent: Scene '{AttackScene.ResourcePath}' does not implement IAttack");
            node.QueueFree();
            return;
        }

        GetTree().CurrentScene.AddChild(node);
    }
}
