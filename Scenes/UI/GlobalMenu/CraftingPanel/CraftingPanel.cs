using System.Linq;
using Godot;
using RotOfTime.Autoload;
using RotOfTime.Core.Artifacts;

public partial class CraftingPanel : Control
{
    private static readonly string[] CraftablePaths =
    [
        "res://Core/Artifacts/EscudoDeGrafito.tres",
        "res://Core/Artifacts/LenteDeFoco.tres",
        "res://Core/Artifacts/NucleoDenso.tres",
    ];

    [Export] public PackedScene ArtifactRowScene;
    [Export] public VBoxContainer CraftListContainer;

    public void Open() => Refresh();

    public void Refresh()
    {
        foreach (Node child in CraftListContainer.GetChildren())
            child.QueueFree();

        var am = GameManager.Instance.ArtifactManager;
        var eco = GameManager.Instance.EconomyManager;

        foreach (string path in CraftablePaths)
        {
            var artifact = GD.Load<ArtifactData>(path);
            if (artifact == null) continue;

            bool alreadyOwned = am.Owned.Any(a => a.ResourcePath == path);
            bool canAfford = eco.Isotopes >= artifact.IsotopeCost;
            var row = ArtifactRowScene.Instantiate<ArtifactRow>();
            var captured = artifact;

            string displayName = alreadyOwned
                ? $"{artifact.ArtifactName} (obtenido)"
                : $"{artifact.ArtifactName} — {artifact.IsotopeCost} iso";

            row.Setup(
                artifactData: artifact,
                onPressed: () =>
                {
                    if (!eco.SpendIsotopes(captured.IsotopeCost)) return;
                    am.AddOwned(captured);
                    GameManager.Instance.SaveMeta();
                    Refresh();
                }
            );

            CraftListContainer.AddChild(row);
        }
    }
}
