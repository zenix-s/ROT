using System.Collections.Generic;
using System.Linq;
using Godot;

namespace RotOfTime.Core.Artifacts;

/// <summary>
///     Manages artifact ownership and equipment.
///     Plain C# class owned by GameManager (not a Godot Node).
///
///     Slot system: player starts with 1 slot, max 3 (unlocked at Elevations 3 and 5).
///     Each artifact costs 1-3 slots. Equip/unequip validated against available slots.
/// </summary>
public class ArtifactManager
{
    private readonly List<ArtifactData> _equipped = new();
    private readonly List<ArtifactData> _owned = new();

    public int MaxSlots { get; set; } = 1;

    public int UsedSlots => _equipped.Sum(a => a.SlotCost);

    public IReadOnlyList<ArtifactData> Equipped => _equipped;
    public IReadOnlyList<ArtifactData> Owned => _owned;

    public bool CanEquip(ArtifactData artifact)
    {
        return artifact != null
            && _owned.Contains(artifact)
            && !_equipped.Contains(artifact)
            && UsedSlots + artifact.SlotCost <= MaxSlots;
    }

    public bool Equip(ArtifactData artifact)
    {
        if (!CanEquip(artifact))
            return false;

        _equipped.Add(artifact);
        GD.Print($"ArtifactManager: Equipped '{artifact.ArtifactName}' ({artifact.SlotCost} slots)");
        return true;
    }

    public bool Unequip(ArtifactData artifact)
    {
        if (artifact == null || !_equipped.Remove(artifact))
            return false;

        GD.Print($"ArtifactManager: Unequipped '{artifact.ArtifactName}'");
        return true;
    }

    public void AddOwned(ArtifactData artifact)
    {
        if (artifact != null && !_owned.Contains(artifact))
        {
            _owned.Add(artifact);
            GD.Print($"ArtifactManager: Acquired '{artifact.ArtifactName}'");
        }
    }

    public float GetTotalHealthBonus()
    {
        return _equipped.Sum(a => a.HealthBonus);
    }

    public float GetTotalDamageBonus()
    {
        return _equipped.Sum(a => a.DamageBonus);
    }

    /// <summary>Returns resource paths of owned artifacts for persistence.</summary>
    public List<string> GetOwnedPaths() => _owned.Select(a => a.ResourcePath).ToList();

    /// <summary>Returns resource paths of equipped artifacts for persistence.</summary>
    public List<string> GetEquippedPaths() => _equipped.Select(a => a.ResourcePath).ToList();

    /// <summary>Restores artifacts from saved resource paths.</summary>
    public void LoadFromPaths(IEnumerable<string> ownedPaths, IEnumerable<string> equippedPaths)
    {
        _owned.Clear();
        _equipped.Clear();

        foreach (string path in ownedPaths)
        {
            var artifact = GD.Load<ArtifactData>(path);
            if (artifact != null)
                _owned.Add(artifact);
            else
                GD.PrintErr($"ArtifactManager: Failed to load artifact at '{path}'");
        }

        foreach (string path in equippedPaths)
        {
            var artifact = GD.Load<ArtifactData>(path);
            if (artifact != null && _owned.Contains(artifact))
                _equipped.Add(artifact);
            else
                GD.PrintErr($"ArtifactManager: Failed to equip artifact at '{path}'");
        }
    }
}
