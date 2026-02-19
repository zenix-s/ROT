using Godot;
using RotOfTime.Autoload;
using RotOfTime.Core.Entities;

namespace RotOfTime.Scenes.UI;

/// <summary>
///     Bonfire UI: activate resonances and advance elevation.
///     Instantiated on demand by Bonfire.cs. Destroys itself on close.
///     Player input is blocked via GameManager.IsMenuOpen while open.
/// </summary>
public partial class BonfireMenu : CanvasLayer
{
    private Label _resonancesLabel;
    private Button _activateButton;
    private Label _elevationLabel;
    private Button _elevationButton;
    private Button _closeButton;

    private Player.Player _player;

    public override void _Ready()
    {
        _resonancesLabel = GetNode<Label>("Panel/VBoxContainer/ResonancesLabel");
        _activateButton = GetNode<Button>("Panel/VBoxContainer/ActivateButton");
        _elevationLabel = GetNode<Label>("Panel/VBoxContainer/ElevationLabel");
        _elevationButton = GetNode<Button>("Panel/VBoxContainer/ElevationButton");
        _closeButton = GetNode<Button>("Panel/VBoxContainer/CloseButton");

        _activateButton.Pressed += OnActivatePressed;
        _elevationButton.Pressed += OnElevationPressed;
        _closeButton.Pressed += OnClosePressed;
    }

    public void Open()
    {
        AddToGroup(Groups.BonfireMenu);
        _player = GetTree().GetFirstNodeInGroup(Groups.Player) as Player.Player;
        GameManager.Instance.IsMenuOpen = true;
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

    private void OnClosePressed()
    {
        Close();
    }
}
