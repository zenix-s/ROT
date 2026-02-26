using Godot;
using RotOfTime.Autoload;
using RotOfTime.Core.Artifacts;

public partial class CraftingPanel : Control
{
    [Export] public PackedScene CraftingRowScene;
    [Export] public VBoxContainer CraftListContainer;

    public void Open() => Refresh();

    public void Refresh()
    {
        foreach (Node child in CraftListContainer.GetChildren())
            child.QueueFree();

        foreach (ArtifactType type in ArtifactManager.ResourcePaths.Keys)
        {
            var artifact = type.LoadData();
            if (artifact == null) continue;

            var row = CraftingRow.Create(CraftingRowScene, type);
            row.ActionPerformed += () =>
            {
                GameManager.Instance.SaveMeta();
                Refresh();
            };
            CraftListContainer.AddChild(row);
        }
    }
}
