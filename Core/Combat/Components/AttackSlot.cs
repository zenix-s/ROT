using Godot;
using RotOfTime.Core.Combat.Attacks;
using RotOfTime.Core.Combat.Components.AttackSpawnerComponents;
using RotOfTime.Core.Entities;

namespace RotOfTime.Core.Combat.Components;

/// <summary>
///     A black-box Node that encapsulates everything needed to execute an attack:
///     attack definition (AttackData), spawn pattern (Spawner), and lifecycle state.
///     The AttackManager only sees this public interface and delegates all
///     execution details to the slot.
///     Config (cooldown, cast, movement) comes from the AttackData Resource.
/// </summary>
[GlobalClass]
public partial class AttackSlot : Node
{
    private float _cooldownRemaining;
    private float _castElapsed;
    private bool _isActive;
    private bool _castCompleted;

    /// <summary>
    ///     The attack definition: damage stats, timing, and scene reference.
    ///     This is the single source of truth for what this slot does.
    /// </summary>
    [Export]
    public AttackData AttackData { get; set; }

    /// <summary>
    ///     The spawner component that handles attack instantiation pattern (SingleShot, Burst, etc.).
    /// </summary>
    [Export]
    public AttackSpawnerComponent Spawner { get; set; }

    // --- Config derived from AttackData ---

    public float Cooldown => AttackData?.CooldownDuration ?? 0f;
    public float CastDuration => AttackData?.CastDuration ?? 0f;
    public bool AllowMovementDuringCast => AttackData?.AllowMovementDuringCast ?? true;
    public bool IsInstantCast => CastDuration <= 0f;

    // --- Runtime state ---

    /// <summary>
    ///     Whether this slot is ready to fire (cooldown expired).
    /// </summary>
    public bool IsReady => _cooldownRemaining <= 0f;

    /// <summary>
    ///     Whether the cast phase has completed for the current activation.
    /// </summary>
    public bool IsCastComplete => _castCompleted;

    /// <summary>
    ///     Whether the entire execution (cast + spawner) has finished.
    /// </summary>
    public bool IsExecutionComplete => !_isActive;

    /// <summary>
    ///     Activate this slot: starts cooldown, cast timer, and triggers the spawner.
    ///     The AttackData is passed to the spawner so it can instantiate the correct scene.
    /// </summary>
    /// <param name="direction">Normalized aim direction</param>
    /// <param name="position">World spawn position</param>
    /// <param name="stats">Owner's entity stats</param>
    /// <param name="ownerNode">The entity node that owns this attack (for position tracking)</param>
    public void Activate(Vector2 direction, Vector2 position, EntityStats stats, Node2D ownerNode)
    {
        if (Spawner == null)
        {
            GD.PrintErr($"AttackSlot '{Name}': No Spawner assigned.");
            return;
        }

        if (AttackData == null)
        {
            GD.PrintErr($"AttackSlot '{Name}': No AttackData assigned.");
            return;
        }

        _cooldownRemaining = Cooldown;
        _castElapsed = 0f;
        _isActive = true;
        _castCompleted = IsInstantCast;

        Spawner.Reset();
        Spawner.Activate(direction, position, stats, AttackData, ownerNode);
    }

    /// <summary>
    ///     Tick cooldown, cast timer, and spawner each frame.
    ///     Called by the AttackManager.
    /// </summary>
    public void Process(double delta)
    {
        var dt = (float)delta;

        // Tick cooldown
        if (_cooldownRemaining > 0f)
            _cooldownRemaining = Mathf.Max(0f, _cooldownRemaining - dt);

        if (!_isActive)
            return;

        // Tick cast elapsed
        _castElapsed += dt;

        // Check cast completion
        if (!_castCompleted && _castElapsed >= CastDuration)
            _castCompleted = true;

        // Tick spawner (for multi-shot patterns like Burst)
        Spawner.Process(delta);

        // Check if the entire execution is done
        if (Spawner.IsComplete)
            _isActive = false;
    }

    /// <summary>
    ///     Reset the slot to idle state. Does not affect cooldown.
    /// </summary>
    public void Reset()
    {
        _isActive = false;
        _castCompleted = false;
        _castElapsed = 0f;
    }

    /// <summary>
    ///     Get the cooldown progress (0 = ready, 1 = just started).
    /// </summary>
    public float GetCooldownProgress()
    {
        if (Cooldown <= 0f)
            return 0f;

        return Mathf.Clamp(_cooldownRemaining / Cooldown, 0f, 1f);
    }
}
