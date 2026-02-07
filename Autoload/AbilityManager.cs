using System.Collections.Generic;
using Godot;
using RotOfTime.Core.Combat;

namespace RotOfTime.Autoload;

/// <summary>
///     Manages player ability loadout.
///     Owned by GameManager, initialized after game data is loaded.
/// </summary>
public class AbilityManager
{
    private readonly Dictionary<StringName, string> _loadout = [];

    public AbilityManager()
    {
        SetupDefaultLoadout();
    }

    public void EquipAbility(StringName key, string scenePath)
    {
        _loadout[key] = scenePath;
    }

    public void UnequipAbility(StringName key)
    {
        _loadout.Remove(key);
    }

    public Dictionary<StringName, string> GetLoadout()
    {
        return new Dictionary<StringName, string>(_loadout);
    }

    public string GetAbilityScene(StringName key)
    {
        return _loadout.GetValueOrDefault(key);
    }

    public bool HasAbility(StringName key)
    {
        return _loadout.ContainsKey(key);
    }

    public void SaveLoadout()
    {
        GD.Print("AbilityManager: SaveLoadout stub - not yet implemented.");
    }

    public void LoadLoadout()
    {
        GD.Print("AbilityManager: LoadLoadout stub - not yet implemented.");
    }

    private void SetupDefaultLoadout()
    {
        EquipAbility(AttackKeys.BasicAttack, "res://Scenes/Attacks/Projectiles/Projectile.tscn");
    }
}
