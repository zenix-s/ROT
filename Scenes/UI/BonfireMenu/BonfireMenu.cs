using System.Linq;
using Godot;
using RotOfTime.Autoload;
using RotOfTime.Core.Artifacts;
using RotOfTime.Core.Entities;

namespace RotOfTime.Scenes.UI;

/// <summary>
///     Bonfire UI: activate resonances, advance elevation, and manage artifacts.
///     Instantiated on demand by Bonfire.cs. Destroys itself on close.
///     Player input is blocked via GameManager.IsMenuOpen while open.
/// </summary>
public partial class BonfireMenu : CanvasLayer
{
    private Panel _mainPanel;
    private Label _resonancesLabel;
    private Button _activateButton;
    private Label _elevationLabel;
    private Button _elevationButton;
    private Button _artifactsButton;
    private Button _closeButton;

    private Panel _artifactsPanel;
    private Label _slotsLabel;
    private VBoxContainer _artifactsListContainer;
    private Button _backButton;

    private Player.Player _player;

    public override void _Ready()
    {
        _mainPanel = GetNode<Panel>("Panel");
        _resonancesLabel = GetNode<Label>("Panel/VBoxContainer/ResonancesLabel");
        _activateButton = GetNode<Button>("Panel/VBoxContainer/ActivateButton");
        _elevationLabel = GetNode<Label>("Panel/VBoxContainer/ElevationLabel");
        _elevationButton = GetNode<Button>("Panel/VBoxContainer/ElevationButton");
        _artifactsButton = GetNode<Button>("Panel/VBoxContainer/ArtifactsButton");
        _closeButton = GetNode<Button>("Panel/VBoxContainer/CloseButton");

        _artifactsPanel = GetNode<Panel>("ArtifactsPanel");
        _slotsLabel = GetNode<Label>("ArtifactsPanel/VBoxContainer/SlotsLabel");
        _artifactsListContainer = GetNode<VBoxContainer>("ArtifactsPanel/VBoxContainer/ArtifactsListContainer");
        _backButton = GetNode<Button>("ArtifactsPanel/VBoxContainer/BackButton");

        _activateButton.Pressed += OnActivatePressed;
        _elevationButton.Pressed += OnElevationPressed;
        _artifactsButton.Pressed += OnArtifactsPressed;
        _closeButton.Pressed += OnClosePressed;
        _backButton.Pressed += OnBackPressed;
    }

    public void Open()
    {
        AddToGroup(Groups.BonfireMenu);
        _player = GetTree().GetFirstNodeInGroup(Groups.Player) as Player.Player;
        GameManager.Instance.IsMenuOpen = true;
        _mainPanel.Visible = true;
        _artifactsPanel.Visible = false;
        Visible = true;
        Refresh();
    }

    private void Close()
    {
        GameManager.Instance.IsMenuOpen = false;
        QueueFree();
    }

    private void Refresh()
    {
        var inv = GameManager.Instance.InventoryManager;
        var prog = GameManager.Instance.ProgressionManager;

        int resonances = inv.GetQuantity("resonance");
        _resonancesLabel.Text = $"Resonancias disponibles: {resonances}";
        _activateButton.Disabled = resonances <= 0;

        string elevKey = $"elevation_{prog.CurrentElevation}";
        bool hasElevItem = inv.HasItem(elevKey);
        _elevationLabel.Visible = hasElevItem;
        _elevationButton.Visible = hasElevItem;
        if (hasElevItem)
            _elevationLabel.Text = $"Item de Elevación {prog.CurrentElevation} recogido";
    }

    private void OnActivatePressed()
    {
        var inv = GameManager.Instance.InventoryManager;
        var prog = GameManager.Instance.ProgressionManager;

        if (!inv.RemoveItem("resonance")) return;
        prog.ActivateResonance();
        _player?.ApplyAllMultipliers();
        GameManager.Instance.SaveMeta();
        Refresh();
    }

    private void OnElevationPressed()
    {
        var inv = GameManager.Instance.InventoryManager;
        var prog = GameManager.Instance.ProgressionManager;

        string elevKey = $"elevation_{prog.CurrentElevation}";
        if (!inv.RemoveItem(elevKey)) return;
        prog.AdvanceElevation();
        _player?.ApplyAllMultipliers();
        GameManager.Instance.SaveMeta();
        Refresh();
    }

    private void OnArtifactsPressed()
    {
        _mainPanel.Visible = false;
        _artifactsPanel.Visible = true;
        RefreshArtifacts();
    }

    private void OnBackPressed()
    {
        _artifactsPanel.Visible = false;
        _mainPanel.Visible = true;
    }

    private void RefreshArtifacts()
    {
        var am = GameManager.Instance.ArtifactManager;
        _slotsLabel.Text = $"Artefactos — Slots: {am.UsedSlots}/{am.MaxSlots}";

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

            var capturedArtifact = artifact;
            if (isEquipped)
                btn.Pressed += () =>
                {
                    am.Unequip(capturedArtifact);
                    _player?.ApplyAllMultipliers();
                    GameManager.Instance.SaveMeta();
                    RefreshArtifacts();
                };
            else
                btn.Pressed += () =>
                {
                    am.Equip(capturedArtifact);
                    _player?.ApplyAllMultipliers();
                    GameManager.Instance.SaveMeta();
                    RefreshArtifacts();
                };

            row.AddChild(label);
            row.AddChild(btn);
            _artifactsListContainer.AddChild(row);
        }
    }

    private void OnClosePressed()
    {
        Close();
    }
}
