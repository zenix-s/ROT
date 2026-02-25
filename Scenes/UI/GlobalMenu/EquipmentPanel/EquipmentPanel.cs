using System.Linq;
using Godot;
using RotOfTime.Autoload;
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

        foreach (var artifact in am.Owned)
        {
            bool isEquipped = am.Equipped.Contains(artifact);
            var row = ArtifactRowScene.Instantiate<ArtifactRow>();
            var captured = artifact;

            row.Setup(
                // name: artifact.ArtifactName,
                // buttonText: isEquipped ? "Desequipar" : "Equipar",
                artifactData: artifact,
                onPressed: () =>
                {
                    if (isEquipped) am.Unequip(captured);
                    else am.Equip(captured);
                    player?.ApplyAllMultipliers();
                    GameManager.Instance.SaveMeta();
                    Refresh();
                }
            );

            ArtifactsListContainer.AddChild(row);
        }
    }
}
