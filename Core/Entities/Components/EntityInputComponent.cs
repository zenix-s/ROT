using Godot;
using RotOfTime.Autoload;

namespace RotOfTime.Core.Entities.Components;

[GlobalClass]
public partial class EntityInputComponent : Node
{
    private static bool IsBlocked => GameManager.Instance?.IsMenuOpen == true;

    public Vector2 Direction => IsBlocked
        ? Vector2.Zero
        : Input.GetVector("move_left", "move_right", "move_top", "move_down");

    public bool IsAttackJustPressed => !IsBlocked && Input.IsActionJustPressed("attack");
    public bool IsDashJustPressed => !IsBlocked && Input.IsActionJustPressed("dash");
    public bool IsSkill1JustPressed => !IsBlocked && Input.IsActionJustPressed("skill_1");
    public bool IsSkill2JustPressed => !IsBlocked && Input.IsActionJustPressed("skill_2");
}
