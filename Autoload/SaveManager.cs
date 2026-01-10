using System;
using System.Text.Json;
using Godot;
using RotOfTime.Core.GameData;

namespace RotOfTime.Autoload;

/// <summary>
///     Persistence layer for meta-progression.
///     Handles file I/O only - no game state management.
/// </summary>
public partial class SaveManager : Node
{
    private const string SaveDirectory = "user://saves/";
    private const string MetaFileName = "meta.json";

    public static SaveManager Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;
        EnsureSaveDirectoryExists();
    }

    private void EnsureSaveDirectoryExists()
    {
        using DirAccess dir = DirAccess.Open("user://");
        if (dir != null && !dir.DirExists("saves"))
            dir.MakeDir("saves");
    }

    private string GetMetaPath()
    {
        return $"{SaveDirectory}{MetaFileName}";
    }

    public bool SaveMeta(MetaData data)
    {
        if (data == null)
        {
            GD.PrintErr("SaveManager: Cannot save null MetaData");
            return false;
        }

        try
        {
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            using FileAccess file = FileAccess.Open(GetMetaPath(), FileAccess.ModeFlags.Write);
            file.StoreString(json);

            GD.Print("SaveManager: Meta saved");
            return true;
        }
        catch (Exception e)
        {
            GD.PrintErr($"SaveManager: Failed to save meta - {e.Message}");
            return false;
        }
    }

    public MetaData LoadMeta()
    {
        string path = GetMetaPath();
        if (!FileAccess.FileExists(path))
            return null;

        try
        {
            using FileAccess file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
            string json = file.GetAsText();
            MetaData data = JsonSerializer.Deserialize<MetaData>(json);

            GD.Print("SaveManager: Meta loaded");
            return data;
        }
        catch (Exception e)
        {
            GD.PrintErr($"SaveManager: Failed to load meta - {e.Message}");
            return null;
        }
    }
}
