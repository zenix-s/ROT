using System;
using Godot;
using RotOfTime.Core.Artifacts;

public partial class ArtifactRow : HBoxContainer
{
    [Export] public Label NameLabel;
    [Export] public Button ActionButton;

    public void Setup(ArtifactData artifactData, Action onPressed)
    {
        NameLabel.Text = artifactData.ArtifactName;
        ActionButton.Pressed += onPressed;
    }
}
