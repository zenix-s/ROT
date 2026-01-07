using Godot;
using RotOfTime.Core;

public partial class Main : Node2D
{
    [Export] private CharacterBody2D _player;
    [Export] private Node _worldContainer;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        OnMenuChangeRequested(SceneExtensionManager.MenuScene.Start);

        SceneManager.Instance.SceneChangeRequested += OnSceneChangeRequested;
        SceneManager.Instance.MenuChangeRequested += OnMenuChangeRequested;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    private  void OnMenuChangeRequested(SceneExtensionManager.MenuScene menuScene)
    {
        foreach (Node node in _worldContainer.GetChildren()) node.QueueFree();

        string path = SceneExtensionManager.MenuPaths[menuScene];
        PackedScene scene = GD.Load<PackedScene>(path);
        Node sceneInstance = scene.Instantiate();
        _worldContainer.AddChild(sceneInstance);
    }

    private void OnSceneChangeRequested(SceneExtensionManager.GameScene gameScene)
    {
        foreach (Node node in _worldContainer.GetChildren()) node.QueueFree();

        string path = SceneExtensionManager.ScenePaths[gameScene];
        PackedScene scene = GD.Load<PackedScene>(path);
        Node sceneInstance = scene.Instantiate();
        _worldContainer.AddChild(sceneInstance);

        Marker2D spawnPoint = sceneInstance.GetNode<Marker2D>("SpawnPoint");
        _player.Position = spawnPoint.GlobalPosition;
    }
}
