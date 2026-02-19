using Godot;
using RotOfTime.Autoload;

namespace RotOfTime.Core.Progression;

/// <summary>
///     Area2D pickup. Player walks over it to collect a resonance item.
///     Place in level scenes. Configure collision mask = layer 2 (Player).
/// </summary>
[GlobalClass]
public partial class ResonanceTrigger : Area2D
{
    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D body)
    {
        GameManager.Instance.InventoryManager.AddItem("resonance");
        GameManager.Instance.SaveMeta();
        GD.Print("ResonanceTrigger: Resonance collected!");
        QueueFree();
    }
}
