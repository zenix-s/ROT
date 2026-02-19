using Godot;
using RotOfTime.Autoload;
using RotOfTime.Core;
using RotOfTime.Core.Entities;
using RotOfTime.Scenes.UI.HUD;

namespace RotOfTime.Scenes.Main;

public partial class Main : Node2D
{
    [Export] private Camera2D _camera;

    private bool _isMenuActive = true;
    private Player.Player _player;
    [Export] private Node _worldContainer;
    [Export] private HUD _hud;

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
        if (Input.IsActionJustPressed("save_game"))
            GameManager.Instance.SaveMeta();
    }

    private void OnMenuChangeRequested(SceneExtensionManager.MenuScene menuScene)
    {
        _hud.Teardown();

        // Close any open in-game menus (bonfire, etc.)
        GetTree().GetFirstNodeInGroup(Groups.BonfireMenu)?.QueueFree();
        GameManager.Instance.IsMenuOpen = false;

        if (IsInstanceValid(_player))
        {
            _player.QueueFree();
            _player = null;
        }

        foreach (Node node in _worldContainer.GetChildren()) node.QueueFree();

        string path = SceneExtensionManager.MenuPaths[menuScene];
        PackedScene scene = GD.Load<PackedScene>(path);
        Node sceneInstance = scene.Instantiate();
        _worldContainer.AddChild(sceneInstance);
        _isMenuActive = true;
        SetupCamera();
    }

    private void OnSceneChangeRequested(SceneExtensionManager.GameScene gameScene)
    {
        foreach (Node node in _worldContainer.GetChildren()) node.QueueFree();

        // Load the player
        InstantiatePlayer();

        string path = SceneExtensionManager.ScenePaths[gameScene];
        PackedScene scene = GD.Load<PackedScene>(path);
        Node sceneInstance = scene.Instantiate();
        _worldContainer.AddChild(sceneInstance);

        Marker2D spawnPoint = sceneInstance.GetNode<Marker2D>("SpawnPoint");
        _player.Position = spawnPoint.GlobalPosition;
        _player.Visible = true;
        _hud.Initialize(_player);

        _isMenuActive = false;
        SetupCamera();
    }

    private void InstantiatePlayer()
    {
        if (_player is not null)
        {
            _player.QueueFree();
            _player = null;
        }

        _player = GD.Load<PackedScene>(
            SceneExtensionManager.EntityPaths[SceneExtensionManager.EntityScene.Player]
        ).Instantiate<Player.Player>();
        _worldContainer.AddChild(_player);
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
