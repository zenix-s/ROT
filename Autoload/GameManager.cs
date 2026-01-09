using System;
using Godot;
using RotOfTime.Core;
using RotOfTime.Core.GameData;

namespace RotOfTime.Autoload;

/// <summary>
///     Runtime state manager for live game data.
///     Holds GameData instance directly, uses SaveManager for persistence.
/// </summary>
public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }

    /// <summary>
    ///     The current game data. Null when no active game.
    /// </summary>
    public GameData Data { get; private set; }

    public bool HasActiveGame => Data != null;

    public override void _Ready()
    {
        Instance = this;
        SceneManager.Instance.SceneChangeRequested += OnSceneChangeRequested;
    }

    public override void _Process(double delta)
    {
        if (HasActiveGame)
            Data.PlayTimeSeconds += delta;
    }

    private void OnSceneChangeRequested(SceneExtensionManager.GameScene gameScene)
    {
        if (HasActiveGame)
            Save();
    }

    /// <summary>
    ///     Start a new game in the specified slot.
    /// </summary>
    public void NewGame(int slotId)
    {
        Data = new GameData
        {
            SlotId = slotId,
            CreatedAt = DateTime.Now,
            LastSavedAt = DateTime.Now
        };

        Save();
        GD.Print($"GameManager: New game started in slot {slotId}");
    }

    /// <summary>
    ///     Save current game data.
    /// </summary>
    public bool Save()
    {
        if (!HasActiveGame)
        {
            GD.PrintErr("GameManager: No active game to save");
            return false;
        }

        return SaveManager.Instance.Save(Data.SlotId, Data);
    }

    /// <summary>
    ///     Load game from the specified slot.
    /// </summary>
    public bool Load(int slotId)
    {
        GameData data = SaveManager.Instance.Load(slotId);
        if (data == null)
            return false;

        Data = data;
        GD.Print($"GameManager: Loaded game from slot {slotId}");
        return true;
    }

    /// <summary>
    ///     Clear current session (e.g., returning to main menu).
    /// </summary>
    public void ClearSession()
    {
        Data = null;
    }
}
