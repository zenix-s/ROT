using Godot;
using RotOfTime.Scenes.UI;

namespace RotOfTime.Scenes.World;

/// <summary>
///     Interactable bonfire placed in levels.
///     Player presses 'interact' (E) while inside DetectionArea to open BonfireMenu.
/// </summary>
public partial class Bonfire : Node2D
{
    private bool _playerInRange;

    public override void _Ready()
    {
        var area = GetNode<Area2D>("DetectionArea");
        area.BodyEntered += OnBodyEntered;
        area.BodyExited += OnBodyExited;
    }

    public override void _Process(double delta)
    {
        if (_playerInRange && Input.IsActionJustPressed("interact"))
            OpenMenu();
    }

    private void OpenMenu()
    {
        var menuNode = GetTree().GetFirstNodeInGroup("BonfireMenu");
        if (menuNode is BonfireMenu menu)
            menu.Open();
    }

    private void OnBodyEntered(Node2D body)
    {
        _playerInRange = true;
        GD.Print("Bonfire: player in range");
    }

    private void OnBodyExited(Node2D body)
    {
        _playerInRange = false;
    }
}
