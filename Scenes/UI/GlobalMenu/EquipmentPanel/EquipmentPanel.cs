using Godot;
using RotOfTime.Autoload;
using RotOfTime.Core.Artifacts;

public partial class EquipmentPanel : Control
{
    [Export] public PackedScene ArtifactRowScene;
    [Export] public VBoxContainer ArtifactsListContainer;
    [Export] public Label SlotsLabel;

    public void Open() => Refresh();

    public void Refresh()
    {
        foreach (Node child in ArtifactsListContainer.GetChildren())
            child.QueueFree();

        var am = GameManager.Instance.ArtifactManager;

        SlotsLabel.Text = $"Slots: {am.UsedSlots}/{am.MaxSlots}";

        foreach (ArtifactType type in am.Owned)
        {
            var row = ArtifactRow.Create(ArtifactRowScene, type);
            row.ActionPerformed += () =>
            {
                GameManager.Instance.SaveMeta();
                Refresh();
            };
            ArtifactsListContainer.AddChild(row);
        }
    }
}
