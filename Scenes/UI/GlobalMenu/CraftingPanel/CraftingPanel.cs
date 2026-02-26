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

        var eco = GameManager.Instance.EconomyManager;

        foreach (ArtifactType type in ArtifactManager.ResourcePaths.Keys)
        {
            var artifact = type.LoadData();
            if (artifact == null) continue;

            var row = ArtifactRowScene.Instantiate<ArtifactRow>();
            var capturedType = type;
            var capturedCost = artifact.IsotopeCost;

            row.Setup(
                artifactData: artifact,
                onPressed: () =>
                {
                    if (!eco.SpendIsotopes(capturedCost)) return;
                    capturedType.AddOwned();
                    GameManager.Instance.SaveMeta();
                    Refresh();
                }
            );

            CraftListContainer.AddChild(row);
        }
    }
}
