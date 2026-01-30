using Godot;
using RotOfTime.Autoload;

namespace RotOfTime.Scenes.Main;

public partial class SaveButton : Button
{
    public override void _Pressed()
    {
        GameManager.Instance.SaveMeta();
    }
}
