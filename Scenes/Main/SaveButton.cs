using Godot;
using RotOfTime.Autoload;

public partial class SaveButton : Button
{
    public override void _Pressed()
    {
        GameManager.Instance.SaveMeta();
    }
}
