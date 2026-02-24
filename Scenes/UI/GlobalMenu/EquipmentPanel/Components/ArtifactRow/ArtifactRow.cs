using System;
using Godot;

public partial class ArtifactRow : HBoxContainer
{
    private const string EquipedText = "Equipado";
    private const string EquipText = "Equipar";

    [Export] public Label NameLabel;
    [Export] public Button ActionButton;

    public override void _Ready()
    {
        NameLabel.Text = "Item";
        ActionButton.Text = GetActionButtonText(false);
    }

    private string GetActionButtonText(bool equipped) => equipped ? EquipedText : EquipText;

    public void Setup(string name, bool equipped, Action onPressed)
    {
        NameLabel.Text = name;
        ActionButton.Text = GetActionButtonText(equipped: equipped);
        ActionButton.Pressed += onPressed;
    }
}
