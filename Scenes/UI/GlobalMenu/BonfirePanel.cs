using Godot;
using RotOfTime.Autoload;
using RotOfTime.Core.Entities;

namespace RotOfTime.Scenes.UI;

/// <summary>
///     Tab de Hoguera dentro del GlobalMenu.
///     Gestiona activación de Resonancias y avance de Elevación.
/// </summary>
public partial class BonfirePanel : VBoxContainer
{
    private Label _resonancesLabel;
    private Button _activateButton;
    private Label _elevationLabel;
    private Button _elevationButton;

    public override void _Ready()
    {
        _resonancesLabel = GetNode<Label>("ResonancesLabel");
        _activateButton = GetNode<Button>("ActivateButton");
        _elevationLabel = GetNode<Label>("ElevationLabel");
        _elevationButton = GetNode<Button>("ElevationButton");

        _activateButton.Pressed += OnActivatePressed;
        _elevationButton.Pressed += OnElevationPressed;
    }

    public void Refresh()
    {
        var inv = GameManager.Instance.InventoryManager;
        var prog = GameManager.Instance.ProgressionManager;

        int resonances = inv.GetQuantity("resonance");
        _resonancesLabel.Text = $"Resonancias disponibles: {resonances}";
        _activateButton.Disabled = resonances <= 0;

        bool hasElevItem = inv.HasItem("elevation");
        bool canAdvance = prog.CanAdvanceElevation();
        _elevationLabel.Visible = hasElevItem;
        _elevationButton.Visible = hasElevItem;
        _elevationButton.Disabled = !canAdvance;
        if (hasElevItem)
        {
            string resonanceStatus = prog.CanAdvanceElevation()
                ? "listo para ascender"
                : $"resonancias: {prog.ActivatedResonances % 3}/3";
            _elevationLabel.Text = $"Item de Elevación recogido ({resonanceStatus})";
        }
    }

    private void OnActivatePressed()
    {
        var inv = GameManager.Instance.InventoryManager;
        var prog = GameManager.Instance.ProgressionManager;

        if (!inv.RemoveItem("resonance")) return;
        prog.ActivateResonance();
        var player = GetTree().GetFirstNodeInGroup(Groups.Player) as Player.Player;
        player?.ApplyAllMultipliers();
        GameManager.Instance.SaveMeta();
        Refresh();
    }

    private void OnElevationPressed()
    {
        var inv = GameManager.Instance.InventoryManager;
        var prog = GameManager.Instance.ProgressionManager;

        if (!inv.HasItem("elevation") || !prog.CanAdvanceElevation()) return;
        inv.RemoveItem("elevation");
        prog.AdvanceElevation();
        var player = GetTree().GetFirstNodeInGroup(Groups.Player) as Player.Player;
        player?.ApplyAllMultipliers();
        GameManager.Instance.SaveMeta();
        Refresh();
    }
}
