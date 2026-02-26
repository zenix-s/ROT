using Godot;
using RotOfTime.Autoload;
using RotOfTime.Core.Artifacts;

public partial class CraftingPanel : Control
{
    [Export] public PackedScene ArtifactRowScene;
    [Export] public VBoxContainer CraftListContainer;

    public void Open() => Refresh();

    public void Refresh()
    {
        foreach (Node child in CraftListContainer.GetChildren())
            child.QueueFree();

        var am = GameManager.Instance.ArtifactManager;
        var eco = GameManager.Instance.EconomyManager;

        foreach (ArtifactType type in ArtifactManager.ResourcePaths.Keys)
        {
            var artifact = ArtifactManager.LoadData(type);
            if (artifact == null) continue;

            bool alreadyOwned = am.IsOwned(type);
            bool canAfford = eco.Isotopes >= artifact.IsotopeCost;
            var row = ArtifactRowScene.Instantiate<ArtifactRow>();
            var capturedType = type;
            var capturedCost = artifact.IsotopeCost;

            row.Setup(
                artifactData: artifact,
                onPressed: () =>
                {
                    if (!eco.SpendIsotopes(capturedCost)) return;
                    am.AddOwned(capturedType);
                    GameManager.Instance.SaveMeta();
                    Refresh();
                }
            );

            CraftListContainer.AddChild(row);
        }
    }
}
