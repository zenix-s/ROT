using Godot;
using RotOfTime.Autoload;

namespace RotOfTime.Core.Progression;

/// <summary>
///     Area2D pickup dropped by a boss. Allows advancing elevation at the bonfire.
///     Export: set Elevation to match the boss's elevation number (1, 2, 3...).
/// </summary>
[GlobalClass]
public partial class ElevationItem : Area2D
{
    [Export] public int Elevation { get; set; } = 1;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is not Scenes.Player.Player) return;
        string itemId = $"elevation_{Elevation}";
        GameManager.Instance.InventoryManager.AddItem(itemId);
        GameManager.Instance.SaveMeta();
        GD.Print($"ElevationItem: elevation_{Elevation} collected!");
        QueueFree();
    }
}
