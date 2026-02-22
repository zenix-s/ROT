using System.Linq;
using Godot;
using RotOfTime.Autoload;
using RotOfTime.Core.Artifacts;
using RotOfTime.Core.Entities;

namespace RotOfTime.Scenes.UI;

/// <summary>
///     Mesa de Artefactos — toggle con tecla K (input action "artifacts_menu").
///     Siempre presente en Main.tscn bajo UI. Visible=false por defecto.
///     Tab Equipar: equip/unequip de artefactos poseídos.
///     Tab Craftear: craftear artefactos con isótopos.
/// </summary>
public partial class ArtifactsMenu : CanvasLayer
{
    private static readonly string[] CraftablePaths =
    [
        "res://Resources/Artifacts/EscudoDeGrafito.tres",
        "res://Resources/Artifacts/LenteDeFoco.tres",
        "res://Resources/Artifacts/NucleoDenso.tres",
    ];

    private VBoxContainer _equipPanel;
    private Label _slotsLabel;
    private VBoxContainer _artifactsListContainer;

    private VBoxContainer _craftPanel;
    private VBoxContainer _craftListContainer;

    private Player.Player _player;

    public override void _Ready()
    {
        _equipPanel = GetNode<VBoxContainer>("Container/Panel/VBoxContainer/EquipPanel");
        _slotsLabel = GetNode<Label>("Container/Panel/VBoxContainer/EquipPanel/SlotsLabel");
        _artifactsListContainer = GetNode<VBoxContainer>("Container/Panel/VBoxContainer/EquipPanel/ArtifactsListContainer");

        _craftPanel = GetNode<VBoxContainer>("Container/Panel/VBoxContainer/CraftPanel");
        _craftListContainer = GetNode<VBoxContainer>("Container/Panel/VBoxContainer/CraftPanel/CraftListContainer");

        GetNode<Button>("Container/Panel/VBoxContainer/TabsRow/EquipTabButton").Pressed += OnEquipTabPressed;
        GetNode<Button>("Container/Panel/VBoxContainer/TabsRow/CraftTabButton").Pressed += OnCraftTabPressed;

        Visible = false;
    }

    public override void _Input(InputEvent @event)
    {
        if (!Input.IsActionJustPressed("artifacts_menu")) return;

        if (Visible)
            Close();
        else if (!GameManager.Instance.IsMenuOpen)
            Open();
    }

    private void Open()
    {
        _player = GetTree().GetFirstNodeInGroup(Groups.Player) as Player.Player;
        GameManager.Instance.IsMenuOpen = true;
        _equipPanel.Visible = true;
        _craftPanel.Visible = false;
        Visible = true;
        RefreshEquip();
    }

    private void Close()
    {
        GameManager.Instance.IsMenuOpen = false;
        Visible = false;
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

        foreach (ArtifactData artifact in am.Owned)
        {
            var row = new HBoxContainer();

            string hpText = artifact.HealthBonus > 0 ? $" +{artifact.HealthBonus * 100:F0}%HP" : "";
            string dmgText = artifact.DamageBonus > 0 ? $" +{artifact.DamageBonus * 100:F0}%DMG" : "";

            var label = new Label
            {
                Text = $"{artifact.ArtifactName} ({artifact.SlotCost}sl){hpText}{dmgText}",
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };

            bool isEquipped = am.Equipped.Contains(artifact);
            var btn = new Button
            {
                Text = isEquipped ? "Desequipar" : "Equipar",
                Disabled = !isEquipped && !am.CanEquip(artifact)
            };

            var captured = artifact;
            if (isEquipped)
                btn.Pressed += () =>
                {
                    am.Unequip(captured);
                    _player?.ApplyAllMultipliers();
                    GameManager.Instance.SaveMeta();
                    RefreshEquip();
                };
            else
                btn.Pressed += () =>
                {
                    am.Equip(captured);
                    _player?.ApplyAllMultipliers();
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
        var am = GameManager.Instance.ArtifactManager;
        var eco = GameManager.Instance.EconomyManager;

        foreach (Node child in _craftListContainer.GetChildren())
            child.QueueFree();

        foreach (string path in CraftablePaths)
        {
            var artifact = GD.Load<ArtifactData>(path);
            if (artifact == null) continue;

            bool alreadyOwned = am.Owned.Any(a => a.ResourcePath == path);
            var row = new HBoxContainer();

            var label = new Label
            {
                Text = alreadyOwned
                    ? $"{artifact.ArtifactName} — ya obtenido"
                    : $"{artifact.ArtifactName} — {artifact.IsotopeCost} isótopos",
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };

            var btn = new Button
            {
                Text = "Craftear",
                Disabled = alreadyOwned || eco.Isotopes < artifact.IsotopeCost
            };

            if (!alreadyOwned)
            {
                var captured = artifact;
                btn.Pressed += () =>
                {
                    if (!eco.SpendIsotopes(captured.IsotopeCost)) return;
                    am.AddOwned(captured);
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
