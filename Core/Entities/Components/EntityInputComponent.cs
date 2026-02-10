using Godot;
using RotOfTime.Scenes.Player;

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

    /// <summary>
    ///     Returns the PlayerAttackSlot corresponding to the currently pressed attack input, or null if none.
    /// </summary>
    public PlayerAttackSlot? GetPressedAttackSlot()
    {
        if (Input.IsActionJustPressed("attack"))
            return PlayerAttackSlot.BasicAttack;

        if (Input.IsActionJustPressed("ability_1"))
            return PlayerAttackSlot.Ability1;

        if (Input.IsActionJustPressed("ability_2"))
            return PlayerAttackSlot.Ability2;

        if (Input.IsActionJustPressed("ability_3"))
            return PlayerAttackSlot.Ability3;

        if (Input.IsActionJustPressed("ability_4"))
            return PlayerAttackSlot.Ability4;

        return null;
    }
}
