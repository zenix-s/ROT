using Godot;
using RotOfTime.Autoload;

namespace RotOfTime.Scenes.UI;

/// <summary>
///     Menú global — toggle con K. Solo gestiona visibilidad.
///     TabContainer se encarga del show/hide de paneles automáticamente.
/// </summary>
public partial class GlobalMenu : CanvasLayer
{
    [Export] public TabContainer _tabContainer;
    [Export] public Button _closeButton;
    
    [Export] public EquipmentPanel _equipmentPanel;
    [Export] public CraftingPanel _craftingPanel;

    public override void _Ready()
    {
        _tabContainer.TabChanged += index => {
            // if (index == 0) ElevationPanel.Open();
            if (index == 3) _equipmentPanel.Open();
            else if (index == 2) _craftingPanel.Open();
            // else if (index == 3) SkillsPanel.Open();
        };
        _closeButton.Pressed += Close;
        
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
        Visible = true;
        // RefreshActiveTab();
    }

    private void Close()
    {
        GameManager.Instance.IsMenuOpen = false;
        Visible = false;
    }

    // private void OnTabChanged(long _) => RefreshActiveTab();

    // private void RefreshActiveTab()
    // {
    //     var panel = _tabContainer.GetCurrentTabControl();
    //     if (panel is ElevationPanel ep) ep.Refresh();
    //     else if (panel is ArtifactsPanel ap) ap.Open();
    //     else if (panel is DashPanel dp) dp.Open();
    // }
}
