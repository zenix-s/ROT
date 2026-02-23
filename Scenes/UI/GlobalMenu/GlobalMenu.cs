using Godot;
using RotOfTime.Autoload;

namespace RotOfTime.Scenes.UI;

/// <summary>
///     Menú global del juego — toggle con K (input action "artifacts_menu").
///     Contiene tabs: Hoguera (progresión) y Artefactos (equip/craft).
///     Siempre presente en Main.tscn. Visible=false por defecto.
/// </summary>
public partial class GlobalMenu : CanvasLayer
{
    private BonfirePanel _bonfirePanel;
    private ArtifactsPanel _artifactsPanel;
    private DashPanel _dashPanel;

    public override void _Ready()
    {
        _bonfirePanel = GetNode<BonfirePanel>("Container/Panel/VBoxContainer/BonfirePanel");
        _artifactsPanel = GetNode<ArtifactsPanel>("Container/Panel/VBoxContainer/ArtifactsPanel");
        _dashPanel = GetNode<DashPanel>("Container/Panel/VBoxContainer/DashPanel");

        GetNode<Button>("Container/Panel/VBoxContainer/TabsRow/BonfireTabButton").Pressed += OnBonfireTabPressed;
        GetNode<Button>("Container/Panel/VBoxContainer/TabsRow/ArtifactsTabButton").Pressed += OnArtifactsTabPressed;
        GetNode<Button>("Container/Panel/VBoxContainer/TabsRow/DashTabButton").Pressed += OnDashTabPressed;
        GetNode<Button>("Container/Panel/VBoxContainer/CloseButton").Pressed += Close;

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
        GameManager.Instance.IsMenuOpen = true;
        ShowTab(_bonfirePanel);
        _bonfirePanel.Refresh();
        Visible = true;
    }

    private void Close()
    {
        GameManager.Instance.IsMenuOpen = false;
        Visible = false;
    }

    private void OnBonfireTabPressed()
    {
        ShowTab(_bonfirePanel);
        _bonfirePanel.Refresh();
    }

    private void OnArtifactsTabPressed()
    {
        ShowTab(_artifactsPanel);
        _artifactsPanel.Open();
    }

    private void OnDashTabPressed()
    {
        ShowTab(_dashPanel);
        _dashPanel.Open();
    }

    private void ShowTab(Control active)
    {
        _bonfirePanel.Visible = active == _bonfirePanel;
        _artifactsPanel.Visible = active == _artifactsPanel;
        _dashPanel.Visible = active == _dashPanel;
    }
}
