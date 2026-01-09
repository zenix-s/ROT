using Godot;
using RotOfTime.Autoload;
using RotOfTime.Core;

namespace RotOfTime.Scenes.Main;

public partial class Main : Node2D
{
    [Export] private Camera2D _camera;

    private bool _isMenuActive = true;
    [Export] private CharacterBody2D _player;
    [Export] private Node _worldContainer;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        OnMenuChangeRequested(SceneExtensionManager.MenuScene.Start);

        SceneManager.Instance.SceneChangeRequested += OnSceneChangeRequested;
        SceneManager.Instance.MenuChangeRequested += OnMenuChangeRequested;

        SetupCamera();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        CameraFollowPlayer();
    }

    private void OnMenuChangeRequested(SceneExtensionManager.MenuScene menuScene)
    {
        foreach (Node node in _worldContainer.GetChildren()) node.QueueFree();

        string path = SceneExtensionManager.MenuPaths[menuScene];
        PackedScene scene = GD.Load<PackedScene>(path);
        Node sceneInstance = scene.Instantiate();
        _worldContainer.AddChild(sceneInstance);
        _isMenuActive = true;
        SetupCamera();
        _player.Position = Vector2.Zero;
        _player.Visible = false;
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
        _player.Visible = true;

        _isMenuActive = false;
        SetupCamera();
    }


    #region Camera

    private void SetupCamera()
    {
        if (_camera == null || _player == null) return;
        if (_isMenuActive)
            SetCameraOnCenter();
        else
            _camera.Position = _player.Position;
    }

    private void SetCameraOnCenter()
    {
        Vector2 viewportSize = GetViewport().GetVisibleRect().Size;
        _camera.Position = viewportSize / 2;
    }

    private void CameraFollowPlayer()
    {
        if (_camera == null || _player == null) return;
        if (_isMenuActive) return;

        _camera.Position = _player.Position;
    }

    #endregion
}
