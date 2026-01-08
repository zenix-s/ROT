using System;
using System.Text.Json;
using Godot;
using RotOfTime.Core.GameData;

/// <summary>
/// Persistence layer for save/load operations.
/// Handles file I/O only - no game state management.
/// </summary>
public partial class SaveManager : Node
{
    [Signal]
    public delegate void SaveCompletedEventHandler(int slotId);

    [Signal]
    public delegate void LoadCompletedEventHandler(int slotId);

    [Signal]
    public delegate void SaveDeletedEventHandler(int slotId);

    private const string SaveDirectory = "user://saves/";
    private const int MaxSlots = 3;

    public static SaveManager Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;
        EnsureSaveDirectoryExists();
    }

    private void EnsureSaveDirectoryExists()
    {
        using var dir = DirAccess.Open("user://");
        if (dir != null && !dir.DirExists("saves"))
            dir.MakeDir("saves");
    }

    private string GetSavePath(int slotId) => $"{SaveDirectory}save_{slotId}.json";

    public bool SaveExists(int slotId)
    {
        return FileAccess.FileExists(GetSavePath(slotId));
    }

    public bool Save(int slotId, GameData data)
    {
        if (data == null)
        {
            GD.PrintErr("Cannot save null GameData");
            return false;
        }

        try
        {
            data.SlotId = slotId;
            data.LastSavedAt = DateTime.Now;

            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            using var file = FileAccess.Open(GetSavePath(slotId), FileAccess.ModeFlags.Write);
            file.StoreString(json);

            GD.Print($"SaveManager: Saved to slot {slotId}");
            EmitSignal(SignalName.SaveCompleted, slotId);
            return true;
        }
        catch (Exception e)
        {
            GD.PrintErr($"SaveManager: Failed to save - {e.Message}");
            return false;
        }
    }

    public GameData Load(int slotId)
    {
        var path = GetSavePath(slotId);
        if (!FileAccess.FileExists(path))
        {
            GD.PrintErr($"SaveManager: No save file at slot {slotId}");
            return null;
        }

        try
        {
            using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
            var json = file.GetAsText();
            var data = JsonSerializer.Deserialize<GameData>(json);

            GD.Print($"SaveManager: Loaded slot {slotId}");
            EmitSignal(SignalName.LoadCompleted, slotId);
            return data;
        }
        catch (Exception e)
        {
            GD.PrintErr($"SaveManager: Failed to load slot {slotId} - {e.Message}");
            return null;
        }
    }

    public GameData[] GetAllSlotInfo()
    {
        var slots = new GameData[MaxSlots];
        for (int i = 0; i < MaxSlots; i++)
        {
            var path = GetSavePath(i + 1);
            if (!FileAccess.FileExists(path))
                continue;

            try
            {
                using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
                var json = file.GetAsText();
                slots[i] = JsonSerializer.Deserialize<GameData>(json);
            }
            catch (Exception e)
            {
                GD.PrintErr($"SaveManager: Failed to read slot {i + 1} info - {e.Message}");
            }
        }
        return slots;
    }

    public bool DeleteSave(int slotId)
    {
        var path = GetSavePath(slotId);
        if (!FileAccess.FileExists(path))
            return false;

        try
        {
            using var dir = DirAccess.Open(SaveDirectory);
            dir.Remove($"save_{slotId}.json");

            GD.Print($"SaveManager: Deleted slot {slotId}");
            EmitSignal(SignalName.SaveDeleted, slotId);
            return true;
        }
        catch (Exception e)
        {
            GD.PrintErr($"SaveManager: Failed to delete slot {slotId} - {e.Message}");
            return false;
        }
    }
}
