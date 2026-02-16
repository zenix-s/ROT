using Godot;

namespace RotOfTime.Core.Entities.Components;

[GlobalClass]
public partial class EntityInputComponent : Node
{
    public Vector2 Direction => Input.GetVector("move_left", "move_right", "move_top", "move_down");
    public bool IsAttackJustPressed => Input.IsActionJustPressed("attack");
    public bool IsDashJustPressed => Input.IsActionJustPressed("dash");

    public bool IsSkill1JustPressed => Input.IsActionJustPressed("skill_1");
    public bool IsSkill2JustPressed => Input.IsActionJustPressed("skill_2");
}
