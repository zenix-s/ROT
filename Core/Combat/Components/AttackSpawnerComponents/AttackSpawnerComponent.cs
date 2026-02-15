using Godot;
using RotOfTime.Core.Combat.Attacks;
using RotOfTime.Core.Entities;

namespace RotOfTime.Core.Combat.Components.AttackSpawnerComponents;

/// <summary>
///     Abstract base for attack spawner components.
///     Each spawner is a reusable Node that defines a spawn pattern (single shot, burst, etc.)
///     and knows how to instantiate attack scenes.
///     The AttackData (including the scene to spawn) is provided by the parent AttackSlot.
/// </summary>
public abstract partial class AttackSpawnerComponent : Node
{
    /// <summary>
    ///     Multiplier applied to damage for attacks spawned by this pattern.
    ///     Allows spawner patterns to tweak damage (e.g., burst could reduce per-shot damage).
    ///     Default is 1.0 (no modification).
    /// </summary>
    [Export]
    public float DamageMultiplier { get; set; } = 1.0f;

    /// <summary>
    ///     Whether this spawner has finished spawning all attacks for the current activation.
    /// </summary>
    public abstract bool IsComplete { get; }

    /// <summary>
    ///     Called once when the attack is fired. Stores context and may spawn immediately.
    /// </summary>
    /// <param name="direction">Normalized direction the attack is aimed at</param>
    /// <param name="position">World position where the attack spawns</param>
    /// <param name="ownerStats">Stats of the entity that initiated the attack</param>
    /// <param name="attackData">Attack definition with scene reference and stats</param>
    /// <param name="ownerNode">The entity node that owns this attack (for position tracking)</param>
    public abstract void Activate(Vector2 direction, Vector2 position, EntityStats ownerStats, AttackData attackData,
        Node2D ownerNode);

    /// <summary>
    ///     Called every frame while the spawner is active (for multi-shot patterns).
    /// </summary>
    public abstract void Process(double delta);

    /// <summary>
    ///     Reset state for reuse. Spawners live in the scene tree and are reused between activations.
    /// </summary>
    public abstract void Reset();

    /// <summary>
    ///     Shared helper to instantiate an attack scene, position it, add it to the Attacks container,
    ///     and then execute it. The node is fully in the scene tree when Execute() is called.
    /// </summary>
    protected void SpawnAttackInstance(Vector2 direction, Vector2 position, EntityStats ownerStats,
        AttackData attackData)
    {
        if (attackData?.AttackScene == null)
        {
            GD.PrintErr($"AttackSpawnerComponent: AttackScene not assigned in AttackData on {Name}");
            return;
        }

        var node = attackData.AttackScene.Instantiate<Node2D>();

        // Position the node before adding to tree
        node.GlobalPosition = position;
        node.Rotation = direction.Angle();

        // Add to Attacks container in Main scene — _Ready() runs here
        var container = GetTree().Root.GetNode<Node2D>("Main/Attacks");
        container.AddChild(node);

        // Execute after node is in the tree and fully initialized
        if (node is IAttack attack)
        {
            attack.Execute(direction, ownerStats, attackData, DamageMultiplier);
        }
        else
        {
            GD.PrintErr(
                $"AttackSpawnerComponent: Scene '{attackData.AttackScene.ResourcePath}' does not implement IAttack");
            node.QueueFree();
        }
    }
}
