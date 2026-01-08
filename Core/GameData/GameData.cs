using System;
using RotOfTime.Core;

namespace RotOfTime.Core.GameData;

/// <summary>
/// DTO for save/load operations.
/// Pure data class - no business logic.
/// </summary>
public class GameData
{
    public int Version { get; set; } = 1;
    public int SlotId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastSavedAt { get; set; }
    public double PlayTimeSeconds { get; set; }
    public SceneExtensionManager.TowerLevel CurrentLevel { get; set; } = SceneExtensionManager.TowerLevel.Level0;
    public PlayerData Player { get; set; } = new();

    // Formatting helpers for UI display
    public string GetFormattedPlayTime()
    {
        var timeSpan = TimeSpan.FromSeconds(PlayTimeSeconds);
        return timeSpan.Hours > 0
            ? $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}"
            : $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
    }

    public string GetFormattedLastSaved()
    {
        return LastSavedAt.ToString("yyyy-MM-dd HH:mm");
    }
}
