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
///     MetaData guarda ArtifactType (enum/int), nunca res:// paths.
/// </summary>
public class ArtifactManager
{
    /// <summary>Lookup table: ArtifactType → resource path. Fuente de verdad de paths.</summary>
    public static readonly Dictionary<ArtifactType, string> ResourcePaths = new()
    {
        [ArtifactType.EscudoDeGrafito] = "res://Core/Artifacts/EscudoDeGrafito.tres",
        [ArtifactType.LenteDeFoco] = "res://Core/Artifacts/LenteDeFoco.tres",
        [ArtifactType.NucleoDenso] = "res://Core/Artifacts/NucleoDenso.tres",
    };

    private readonly List<ArtifactType> _owned = [];
    private readonly List<ArtifactType> _equipped = [];

    public int MaxSlots { get; set; } = 1;
    public int UsedSlots => _equipped.Sum(t => LoadData(t).SlotCost);

    public IReadOnlyList<ArtifactType> Owned => _owned;
    public IReadOnlyList<ArtifactType> Equipped => _equipped;

    public static ArtifactData LoadData(ArtifactType type) =>
        GD.Load<ArtifactData>(ResourcePaths[type]);

    public bool IsOwned(ArtifactType type) => _owned.Contains(type);
    public bool IsEquipped(ArtifactType type) => _equipped.Contains(type);

    public bool CanEquip(ArtifactType type)
    {
        return _owned.Contains(type)
            && !_equipped.Contains(type)
            && UsedSlots + LoadData(type).SlotCost <= MaxSlots;
    }

    public bool Equip(ArtifactType type)
    {
        if (!CanEquip(type))
            return false;

        _equipped.Add(type);
        return true;
    }

    public bool Unequip(ArtifactType type) => _equipped.Remove(type);

    public void AddOwned(ArtifactType type)
    {
        if (!_owned.Contains(type))
            _owned.Add(type);
    }

    public float GetTotalHealthBonus() => _equipped.Sum(t => LoadData(t).HealthBonus);
    public float GetTotalDamageBonus() => _equipped.Sum(t => LoadData(t).DamageBonus);

    public List<ArtifactType> GetOwned() => [.. _owned];
    public List<ArtifactType> GetEquipped() => [.. _equipped];

    public void Load(IEnumerable<ArtifactType> owned, IEnumerable<ArtifactType> equipped)
    {
        _owned.Clear();
        _equipped.Clear();
        _owned.AddRange(owned);

        foreach (ArtifactType type in equipped)
        {
            if (_owned.Contains(type))
                _equipped.Add(type);
            else
                GD.PrintErr($"ArtifactManager: Artifact {type} not owned, can't equip");
        }
    }
}
