using System.Collections.Generic;
using RotOfTime.Autoload;

namespace RotOfTime.Core.GameData;

/// <summary>
///     Permanent progression that persists across all runs.
///     Saved to meta.json file.
/// </summary>
public class MetaData
{
    public List<Milestone> CompletedMilestones { get; set; } = [];
    public int CurrentElevation { get; set; } = 1;
    public int ActivatedResonances { get; set; }
    public Dictionary<string, int> Inventory { get; set; } = new();
    public int ArtifactMaxSlots { get; set; } = 1;
    public List<string> OwnedArtifacts { get; set; } = [];
    public List<string> EquippedArtifacts { get; set; } = [];
    public int Isotopes { get; set; }
}
