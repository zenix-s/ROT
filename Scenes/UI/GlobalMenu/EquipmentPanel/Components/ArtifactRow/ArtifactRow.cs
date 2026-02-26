using Godot;
using RotOfTime.Core.Artifacts;

public partial class ArtifactRow : HBoxContainer
{
    [Export] public Label NameLabel;
    [Export] public Button ActionButton;

    [Signal] public delegate void ActionPerformedEventHandler();

    private ArtifactType _type;

    public static ArtifactRow Create(PackedScene scene, ArtifactType type)
    {
        var row = scene.Instantiate<ArtifactRow>();
        row._type = type;
        return row;
    }

    public override void _Ready()
    {
        var data = _type.LoadData();
        NameLabel.Text = data.ArtifactName;
        ActionButton.Text = _type.IsEquipped() ? "Desequipar" : "Equipar";
        ActionButton.Disabled = !_type.IsEquipped() && !_type.CanEquip();
        ActionButton.Pressed += OnActionPressed;
    }

    private void OnActionPressed()
    {
        if (_type.IsEquipped()) _type.Unequip();
        else _type.Equip();
        EmitSignal(SignalName.ActionPerformed);
    }
}
