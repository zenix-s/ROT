using Godot;
using RotOfTime.Autoload;

namespace RotOfTime.Core.Economy;

/// <summary>
///     Collectible isotope pickup that adds currency when player walks over it.
///     Spawned by enemies on death.
/// </summary>
public partial class IsotopePickup : Area2D
{
    [Export] public int Amount { get; set; } = 10;

    /// <summary>
    ///     Slight upward float animation speed.
    /// </summary>
    private float _bobTime;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    public override void _Process(double delta)
    {
        // Simple bob animation for visual feedback
        _bobTime += (float)delta * 3f;
        var sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
        if (sprite != null)
            sprite.Position = new Vector2(0, Mathf.Sin(_bobTime) * 2f);
    }

    private void OnBodyEntered(Node2D body)
    {
        if (!body.IsInGroup("Player")) return;

        GameManager.Instance?.EconomyManager?.AddIsotopes(Amount);
        QueueFree();
    }
}
