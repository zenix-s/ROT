using System;
using Godot;
using RotOfTime.Core;
using RotOfTime.Core.GameData;

/// <summary>
/// Runtime state manager for live game data.
/// Uses SaveManager for persistence, GameData as DTO.
/// </summary>
public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }

    // Save slot tracking
    public int? ActiveSlot { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime LastSavedAt { get; private set; }

    // Live game state
    public double PlayTimeSeconds { get; private set; }
    public SceneExtensionManager.TowerLevel CurrentLevel { get; set; } = SceneExtensionManager.TowerLevel.Level0;
    public Vector2 PlayerPosition { get; set; }

    public bool HasActiveGame => ActiveSlot.HasValue;

    public override void _Ready()
    {
        Instance = this;

        // Subscribe to scene changes for auto-save
        SceneManager.Instance.SceneChangeRequested += OnSceneChangeRequested;
    }

    public override void _Process(double delta)
    {
        // Track play time during active session
        if (HasActiveGame)
            PlayTimeSeconds += delta;
    }

    private void OnSceneChangeRequested(SceneExtensionManager.GameScene gameScene)
    {
        // Auto-save when changing game scenes
        if (HasActiveGame)
            Save();
    }

    /// <summary>
    /// Create a GameData DTO from current runtime state.
    /// </summary>
    public GameData ToGameData()
    {
        return new GameData
        {
            SlotId = ActiveSlot ?? 0,
            CreatedAt = CreatedAt,
            LastSavedAt = DateTime.Now,
            PlayTimeSeconds = PlayTimeSeconds,
            CurrentLevel = CurrentLevel,
            Player = new PlayerData { Position = PlayerPosition }
        };
    }

    /// <summary>
    /// Populate runtime state from a GameData DTO.
    /// </summary>
    public void LoadFromGameData(GameData data)
    {
        if (data == null) return;

        ActiveSlot = data.SlotId;
        CreatedAt = data.CreatedAt;
        LastSavedAt = data.LastSavedAt;
        PlayTimeSeconds = data.PlayTimeSeconds;
        CurrentLevel = data.CurrentLevel;
        PlayerPosition = data.Player?.Position ?? Vector2.Zero;
    }

    /// <summary>
    /// Initialize fresh game state for a new game.
    /// </summary>
    public void NewGame(int slotId)
    {
        ActiveSlot = slotId;
        CreatedAt = DateTime.Now;
        LastSavedAt = DateTime.Now;
        PlayTimeSeconds = 0;
        CurrentLevel = SceneExtensionManager.TowerLevel.Level0;
        PlayerPosition = Vector2.Zero;

        Save();
        GD.Print($"GameManager: New game started in slot {slotId}");
    }

    /// <summary>
    /// Save current state via SaveManager.
    /// </summary>
    public bool Save()
    {
        if (!HasActiveGame)
        {
            GD.PrintErr("GameManager: No active game to save");
            return false;
        }

        var data = ToGameData();
        return SaveManager.Instance.Save(ActiveSlot.Value, data);
    }

    /// <summary>
    /// Load game state from SaveManager.
    /// </summary>
    public bool Load(int slotId)
    {
        var data = SaveManager.Instance.Load(slotId);
        if (data == null)
            return false;

        LoadFromGameData(data);
        GD.Print($"GameManager: Loaded game from slot {slotId}");
        return true;
    }

    /// <summary>
    /// Clear current game state (e.g., returning to main menu).
    /// </summary>
    public void ClearSession()
    {
        ActiveSlot = null;
        PlayTimeSeconds = 0;
        CurrentLevel = SceneExtensionManager.TowerLevel.Level0;
        PlayerPosition = Vector2.Zero;
    }

    // Convenience methods for updating state
    public void UpdatePlayerPosition(Vector2 position) => PlayerPosition = position;
    public void UpdateCurrentLevel(SceneExtensionManager.TowerLevel level) => CurrentLevel = level;
}
