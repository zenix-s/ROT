using Godot;

namespace RotOfTime.Core.Entities.Components;

[GlobalClass]
public partial class EntityInputComponent : Node
{
    public Vector2 Direction => Input.GetVector("move_left", "move_right", "move_top", "move_down");
    public bool IsAttackJustPressed => Input.IsActionJustPressed("attack");
    public bool IsDashJustPressed => Input.IsActionJustPressed("dash");

    public bool IsAbility1JustPressed => Input.IsActionJustPressed("ability_1");
    public bool IsAbility2JustPressed => Input.IsActionJustPressed("ability_2");
    public bool IsAbility3JustPressed => Input.IsActionJustPressed("ability_3");
    public bool IsAbility4JustPressed => Input.IsActionJustPressed("ability_4");

    public bool IsAbilityJustPressed(int index) => index switch
    {
        0 => IsAbility1JustPressed,
        1 => IsAbility2JustPressed,
        2 => IsAbility3JustPressed,
        3 => IsAbility4JustPressed,
        _ => false
    };
}
