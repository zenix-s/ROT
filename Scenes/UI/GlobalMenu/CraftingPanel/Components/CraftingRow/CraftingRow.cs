using Godot;
using RotOfTime.Autoload;
using RotOfTime.Core.Artifacts;

public partial class CraftingRow : HBoxContainer
{
    [Export] public Label NameLabel;
    [Export] public Label StatusLabel;
    [Export] public Button ActionButton;

    [Signal] public delegate void ActionPerformedEventHandler();

    private ArtifactType _type;

    public static CraftingRow Create(PackedScene scene, ArtifactType type)
    {
        var row = scene.Instantiate<CraftingRow>();
        row._type = type;
        return row;
    }

    public override void _Ready()
    {
        var data = _type.LoadData();
        var eco = GameManager.Instance.EconomyManager;
        bool isOwned = _type.IsOwned();

        NameLabel.Text = data.ArtifactName;
        StatusLabel.Text = isOwned ? "ya obtenido" : $"{data.IsotopeCost} isótopos";
        ActionButton.Text = "Craftear";
        ActionButton.Disabled = isOwned || eco.Isotopes < data.IsotopeCost;

        if (!isOwned)
            ActionButton.Pressed += OnActionPressed;
    }

    private void OnActionPressed()
    {
        var data = _type.LoadData();
        var eco = GameManager.Instance.EconomyManager;
        if (!eco.SpendIsotopes(data.IsotopeCost)) return;
        _type.AddOwned();
        EmitSignal(SignalName.ActionPerformed);
    }
}
