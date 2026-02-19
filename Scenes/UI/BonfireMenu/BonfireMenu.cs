using Godot;
using RotOfTime.Autoload;

namespace RotOfTime.Scenes.UI;

/// <summary>
///     Bonfire UI: activate resonances and advance elevation.
///     Lives in Main.tscn. Opens via Bonfire.cs (group "BonfireMenu").
///     Pauses the scene tree while open.
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
        AddToGroup("BonfireMenu");
        ProcessMode = ProcessModeEnum.Always;
        Visible = false;

        _resonancesLabel = GetNode<Label>("Panel/VBoxContainer/ResonancesLabel");
        _activateButton = GetNode<Button>("Panel/VBoxContainer/ActivateButton");
        _elevationLabel = GetNode<Label>("Panel/VBoxContainer/ElevationLabel");
        _elevationButton = GetNode<Button>("Panel/VBoxContainer/ElevationButton");
        _closeButton = GetNode<Button>("Panel/VBoxContainer/CloseButton");

        _activateButton.Pressed += OnActivatePressed;
        _elevationButton.Pressed += OnElevationPressed;
        _closeButton.Pressed += OnClosePressed;
    }

    public void Initialize(Player.Player player)
    {
        _player = player;
    }

    public void Open()
    {
        Visible = true;
        GetTree().Paused = true;
        Refresh();
    }

    public void ForceClose()
    {
        Visible = false;
        GetTree().Paused = false;
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
        ForceClose();
    }
}
