using Godot;
using RotOfTime.Core;

public partial class SceneManager : Node
{
    [Signal]
    public delegate void MenuChangeRequestedEventHandler(SceneExtensionManager.MenuScene menuScene);

    [Signal]
    public delegate void SceneChangeRequestedEventHandler(SceneExtensionManager.GameScene gameScene);

    public static SceneManager Instance { get; private set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Instance = this;
    }

    public void RequestSceneChange(SceneExtensionManager.GameScene gameScene)
    {
        EmitSignal(SignalName.SceneChangeRequested, Variant.From(gameScene));
    }

    public void RequestMenuChange(SceneExtensionManager.MenuScene menuScene)
    {
        EmitSignal(SignalName.MenuChangeRequested, Variant.From(menuScene));
    }
}
