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
            var row = ArtifactRowScene.Instantiate<ArtifactRow>();
            var capturedType = type;

            row.Setup(
                artifactData: type.LoadData(),
                onPressed: () =>
                {
                    if (capturedType.IsEquipped()) capturedType.Unequip();
                    else capturedType.Equip();
                    player?.ApplyAllMultipliers();
                    GameManager.Instance.SaveMeta();
                    Refresh();
                }
            );

            ArtifactsListContainer.AddChild(row);
        }
    }
}
