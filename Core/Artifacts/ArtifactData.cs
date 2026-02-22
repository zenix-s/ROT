using Godot;

namespace RotOfTime.Core.Artifacts;

/// <summary>
///     Defines an artifact's properties. Each artifact can grant
///     HP and/or damage bonuses and costs 1-3 slots to equip.
/// </summary>
[GlobalClass]
public partial class ArtifactData : Resource
{
    [Export] public string ArtifactName { get; set; } = "Unnamed Artifact";
    [Export(PropertyHint.MultilineText)] public string Description { get; set; } = "";
    [Export(PropertyHint.Range, "1,3")] public int SlotCost { get; set; } = 1;
    [Export] public float HealthBonus { get; set; }
    [Export] public float DamageBonus { get; set; }
    [Export] public int IsotopeCost { get; set; } = 0;
}
