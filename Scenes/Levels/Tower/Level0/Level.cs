using Godot;
using RotOfTime.Autoload;

namespace RotOfTime.Scenes.Levels.Tower.Level0;

public partial class Level : Node2D
{
    public override void _Ready()
    {
        Milestone.GameStarted.Complete();
    }
}
