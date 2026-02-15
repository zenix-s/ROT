using Godot;

namespace RotOfTime.Core.Entities.Components;

[GlobalClass]
public partial class EntityInputComponent : Node
{
    public Vector2 Direction => Input.GetVector("move_left", "move_right", "move_top", "move_down");
    public bool IsAttackJustPressed => Input.IsActionJustPressed("attack");
    public bool IsDashJustPressed => Input.IsActionJustPressed("dash");

    public bool IsSpell1JustPressed => Input.IsActionJustPressed("spell_1");
    public bool IsSpell2JustPressed => Input.IsActionJustPressed("spell_2");
}
