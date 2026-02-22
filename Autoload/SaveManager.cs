using System;
using System.IO;
using System.Text;
using System.Text.Json;
using Godot;
using RotOfTime.Core.GameData;

namespace RotOfTime.Autoload;

/// <summary>
///     Persistence layer for meta-progression.
///     Handles file I/O only - no game state management.
/// </summary>
public class SaveManager
{
    private string GetSaveDirectory() => ProjectSettings.GlobalizePath("user://saves/");
    private string GetMetaPath() => Path.Combine(GetSaveDirectory(), "meta.json");

    public SaveManager()
    {
        Directory.CreateDirectory(GetSaveDirectory());
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
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(GetMetaPath(), json, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
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
        if (!File.Exists(path))
            return null;

        try
        {
            string json = File.ReadAllText(path, Encoding.UTF8);
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
