using Godot;
using RotOfTime.Autoload;

namespace RotOfTime.Core.Progression;

/// <summary>
///     Area2D pickup dropped by a boss. Adds a generic "elevation" item to inventory.
///     At the bonfire, the player can spend it (+ 3 activated resonances) to advance elevation.
/// </summary>
[GlobalClass]
public partial class ElevationItem : Area2D
{
    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is not Scenes.Player.Player) return;
        GameManager.Instance.InventoryManager.AddItem("elevation");
        GameManager.Instance.SaveMeta();
        QueueFree();
    }
}
