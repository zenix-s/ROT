using Godot;
using RotOfTime.Autoload;
using RotOfTime.Core.Artifacts;
using RotOfTime.Core.Entities;

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
        var player = GetTree().GetFirstNodeInGroup(Groups.Player) as RotOfTime.Scenes.Player.Player;

        if (SlotsLabel != null)
            SlotsLabel.Text = $"Slots: {am.UsedSlots}/{am.MaxSlots}";

        foreach (ArtifactType type in am.Owned)
        {
            var artifact = ArtifactManager.LoadData(type);
            bool isEquipped = am.IsEquipped(type);
            var row = ArtifactRowScene.Instantiate<ArtifactRow>();
            var capturedType = type;

            row.Setup(
                artifactData: artifact,
                onPressed: () =>
                {
                    if (isEquipped) am.Unequip(capturedType);
                    else am.Equip(capturedType);
                    player?.ApplyAllMultipliers();
                    GameManager.Instance.SaveMeta();
                    Refresh();
                }
            );

            ArtifactsListContainer.AddChild(row);
        }
    }
}
