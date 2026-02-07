using Godot;
using RotOfTime.Core.Combat;

namespace RotOfTime.Core.Entities.Components;

[GlobalClass]
public partial class EntityInputComponent : Node
{
    public Vector2 Direction => Input.GetVector("move_left", "move_right", "move_top", "move_down");
    public bool IsAttackJustPressed => Input.IsActionJustPressed("attack");
    public bool IsDashJustPressed => Input.IsActionJustPressed("dash");

    public bool IsAbilityJustPressed(int index) => index switch
    {
        0 => Input.IsActionJustPressed("ability_1"),
        1 => Input.IsActionJustPressed("ability_2"),
        2 => Input.IsActionJustPressed("ability_3"),
        3 => Input.IsActionJustPressed("ability_4"),
        _ => false
    };

    public StringName GetPressedAttackKey()
    {
        if (Input.IsActionJustPressed("attack"))
            return AttackKeys.BasicAttack;

        if (Input.IsActionJustPressed("ability_1"))
            return AttackKeys.Ability1;

        if (Input.IsActionJustPressed("ability_2"))
            return AttackKeys.Ability2;

        if (Input.IsActionJustPressed("ability_3"))
            return AttackKeys.Ability3;

        if (Input.IsActionJustPressed("ability_4"))
            return AttackKeys.Ability4;

        return null;
    }
}
