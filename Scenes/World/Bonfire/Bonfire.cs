using Godot;
using RotOfTime.Autoload;
using RotOfTime.Scenes.UI;

namespace RotOfTime.Scenes.World;

/// <summary>
///     Interactable bonfire placed in levels.
///     Player presses 'interact' (E) while inside DetectionArea to open BonfireMenu.
///     BonfireMenu is instantiated on demand and destroys itself when closed.
/// </summary>
public partial class Bonfire : Node2D
{
    private static readonly string BonfireMenuScene = "res://Scenes/UI/BonfireMenu/BonfireMenu.tscn";
    private bool _playerInRange;

    public override void _Ready()
    {
        var area = GetNode<Area2D>("DetectionArea");
        area.BodyEntered += OnBodyEntered;
        area.BodyExited += OnBodyExited;
    }

    public override void _Process(double delta)
    {
        if (_playerInRange && !GameManager.Instance.IsMenuOpen && Input.IsActionJustPressed("interact"))
            OpenMenu();
    }

    private void OpenMenu()
    {
        var scene = GD.Load<PackedScene>(BonfireMenuScene);
        var menu = scene.Instantiate<BonfireMenu>();
        GetTree().Root.AddChild(menu);
        menu.Open();
    }

    private void OnBodyEntered(Node2D body)
    {
        _playerInRange = true;
    }

    private void OnBodyExited(Node2D body)
    {
        _playerInRange = false;
    }
}
