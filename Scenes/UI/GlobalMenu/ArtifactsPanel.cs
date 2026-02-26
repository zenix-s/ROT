using Godot;
using RotOfTime.Autoload;
using RotOfTime.Core.Artifacts;
using RotOfTime.Core.Entities;

namespace RotOfTime.Scenes.UI;

/// <summary>
///     Tab de Artefactos dentro del GlobalMenu.
///     Sub-tabs: Equipar y Craftear.
/// </summary>
public partial class ArtifactsPanel : VBoxContainer
{
    private VBoxContainer _equipPanel;
    private Label _slotsLabel;
    private VBoxContainer _artifactsListContainer;

    private VBoxContainer _craftPanel;
    private VBoxContainer _craftListContainer;

    public override void _Ready()
    {
        _equipPanel = GetNode<VBoxContainer>("EquipPanel");
        _slotsLabel = GetNode<Label>("EquipPanel/SlotsLabel");
        _artifactsListContainer = GetNode<VBoxContainer>("EquipPanel/ArtifactsListContainer");

        _craftPanel = GetNode<VBoxContainer>("CraftPanel");
        _craftListContainer = GetNode<VBoxContainer>("CraftPanel/CraftListContainer");

        GetNode<Button>("SubTabsRow/EquipTabButton").Pressed += OnEquipTabPressed;
        GetNode<Button>("SubTabsRow/CraftTabButton").Pressed += OnCraftTabPressed;
    }

    public void Open()
    {
        _equipPanel.Visible = true;
        _craftPanel.Visible = false;
        RefreshEquip();
    }

    private void OnEquipTabPressed()
    {
        _equipPanel.Visible = true;
        _craftPanel.Visible = false;
        RefreshEquip();
    }

    private void OnCraftTabPressed()
    {
        _equipPanel.Visible = false;
        _craftPanel.Visible = true;
        RefreshCraft();
    }

    private void RefreshEquip()
    {
        var am = GameManager.Instance.ArtifactManager;
        _slotsLabel.Text = $"Slots: {am.UsedSlots}/{am.MaxSlots}";

        foreach (Node child in _artifactsListContainer.GetChildren())
            child.QueueFree();

        foreach (ArtifactType type in am.Owned)
        {
            var artifact = type.LoadData();
            var row = new HBoxContainer();

            string hpText = artifact.HealthBonus > 0 ? $" +{artifact.HealthBonus * 100:F0}%HP" : "";
            string dmgText = artifact.DamageBonus > 0 ? $" +{artifact.DamageBonus * 100:F0}%DMG" : "";

            var label = new Label
            {
                Text = $"{artifact.ArtifactName} ({artifact.SlotCost}sl){hpText}{dmgText}",
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };

            var btn = new Button
            {
                Text = type.IsEquipped() ? "Desequipar" : "Equipar",
                Disabled = !type.IsEquipped() && !type.CanEquip()
            };

            var capturedType = type;
            if (type.IsEquipped())
                btn.Pressed += () =>
                {
                    capturedType.Unequip();
                    var player = GetTree().GetFirstNodeInGroup(Groups.Player) as Player.Player;
                    player?.ApplyAllMultipliers();
                    GameManager.Instance.SaveMeta();
                    RefreshEquip();
                };
            else
                btn.Pressed += () =>
                {
                    capturedType.Equip();
                    var player = GetTree().GetFirstNodeInGroup(Groups.Player) as Player.Player;
                    player?.ApplyAllMultipliers();
                    GameManager.Instance.SaveMeta();
                    RefreshEquip();
                };

            row.AddChild(label);
            row.AddChild(btn);
            _artifactsListContainer.AddChild(row);
        }
    }

    private void RefreshCraft()
    {
        var eco = GameManager.Instance.EconomyManager;

        foreach (Node child in _craftListContainer.GetChildren())
            child.QueueFree();

        foreach (ArtifactType type in ArtifactManager.ResourcePaths.Keys)
        {
            var artifact = type.LoadData();
            if (artifact == null) continue;

            var row = new HBoxContainer();

            var label = new Label
            {
                Text = type.IsOwned()
                    ? $"{artifact.ArtifactName} — ya obtenido"
                    : $"{artifact.ArtifactName} — {artifact.IsotopeCost} isótopos",
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };

            var btn = new Button
            {
                Text = "Craftear",
                Disabled = type.IsOwned() || eco.Isotopes < artifact.IsotopeCost
            };

            if (!type.IsOwned())
            {
                var capturedType = type;
                var capturedCost = artifact.IsotopeCost;
                btn.Pressed += () =>
                {
                    if (!eco.SpendIsotopes(capturedCost)) return;
                    capturedType.AddOwned();
                    GameManager.Instance.SaveMeta();
                    RefreshCraft();
                };
            }

            row.AddChild(label);
            row.AddChild(btn);
            _craftListContainer.AddChild(row);
        }
    }
}
