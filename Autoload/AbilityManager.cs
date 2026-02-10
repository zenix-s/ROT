using System.Collections.Generic;
using Godot;
using RotOfTime.Scenes.Player;

namespace RotOfTime.Autoload;

/// <summary>
///     Manages player ability loadout.
///     Owned by GameManager, initialized after game data is loaded.
///     Currently uses hardcoded defaults. Will be expanded to support
///     player ability selection and persistence.
/// </summary>
public class AbilityManager
{
    /// <summary>
    ///     Default spawner scene paths mapped to PlayerAttackSlot.
    ///     These are the PackedScene paths for each attack spawner.
    ///     The spawner scenes themselves contain the configuration (cooldown, cast duration, etc.)
    ///     as exported properties.
    /// </summary>
    private readonly Dictionary<PlayerAttackSlot, string> _loadout = [];

    public AbilityManager()
    {
        SetupDefaultLoadout();
    }

    public void EquipAbility(PlayerAttackSlot slot, string spawnerScenePath)
    {
        _loadout[slot] = spawnerScenePath;
    }

    public void UnequipAbility(PlayerAttackSlot slot)
    {
        _loadout.Remove(slot);
    }

    public Dictionary<PlayerAttackSlot, string> GetLoadout()
    {
        return new Dictionary<PlayerAttackSlot, string>(_loadout);
    }

    public string GetAbilityScenePath(PlayerAttackSlot slot)
    {
        return _loadout.GetValueOrDefault(slot);
    }

    public bool HasAbility(PlayerAttackSlot slot)
    {
        return _loadout.ContainsKey(slot);
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
        // TODO: These paths will point to spawner scenes once they're created in the editor.
        // For now the PlayerAttackManager uses [Export] references to spawner nodes
        // that are children in the Player scene tree, so this loadout is not actively
        // used until the dynamic ability equipping system is implemented.
    }
}
