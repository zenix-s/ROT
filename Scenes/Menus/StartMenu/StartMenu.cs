using Godot;
using RotOfTime.Autoload;
using RotOfTime.Core;

namespace RotOfTime.Scenes.Menus.StartMenu;

public partial class StartMenu : Node2D
{
    [Export] private Button _startButton;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _startButton.Pressed += _on_StartButton_pressed;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    private static void _on_StartButton_pressed()
    {
        GD.Print("Start Button Pressed");
        SceneManager.Instance.RequestMenuChange(SceneExtensionManager.MenuScene.SaveSelection);
    }
}
