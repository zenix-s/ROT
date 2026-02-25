using System;
using Godot;
using RotOfTime.Core.Artifacts;

public partial class ArtifactRow : HBoxContainer
{
    [Export] public Label NameLabel;
    [Export] public Button ActionButton;
    
    private string ActionDataText(bool isEquipped) => isEquipped ? "Desequipar" : "Equipar";

    public void Setup(ArtifactData artifactData, Action onPressed)
    {
        NameLabel.Text = artifactData.ArtifactName;
        ActionButton.Text = ActionDataText(false);
        ActionButton.Pressed += onPressed;
    }
}
