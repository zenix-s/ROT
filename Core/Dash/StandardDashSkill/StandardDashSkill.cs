using Godot;
using RotOfTime.Scenes.Player;

namespace RotOfTime.Core.Dash;

/// <summary>
///     Dash horizontal estándar. Delega en EntityMovementComponent.Dash().
/// </summary>
public partial class StandardDashSkill : DashSkill
{
    protected override void Execute(Vector2 direction, Node2D owner)
    {
        if (owner is not Player player) return;
        player.EntityMovementComponent.Dash(direction, Data.Speed);
    }
}
