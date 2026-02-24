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
    
    

    public override void _Ready()
    {
        _tabContainer.TabChanged += OnTabChanged;
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
        RefreshActiveTab();
    }

    private void Close()
    {
        GameManager.Instance.IsMenuOpen = false;
        Visible = false;
    }

    private void OnTabChanged(long _) => RefreshActiveTab();

    private void RefreshActiveTab()
    {
        var panel = _tabContainer.GetCurrentTabControl();
        if (panel is ElevationPanel ep) ep.Refresh();
        else if (panel is ArtifactsPanel ap) ap.Open();
        else if (panel is DashPanel dp) dp.Open();
    }
}
