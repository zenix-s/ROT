using Godot;

namespace RotOfTime.Scenes.Player;

public partial class Player : CharacterBody2D
{
    [Export] public PlayerComponentManager ComponentManager;

    public const float Speed = 200.0f;

    public override void _PhysicsProcess(double delta)
    {
        Vector2 direction = Input.GetVector(
            negativeX: "move_left",
            positiveX: "move_right",
            negativeY: "move_top",
            positiveY: "move_down");

        if (direction.X > 0)
            ComponentManager.AnimationComponent.AnimatedSprite2D.FlipH = false;
        else if (direction.X < 0)
            ComponentManager.AnimationComponent.AnimatedSprite2D.FlipH = true;


        Velocity = direction.Normalized() * Speed;
        MoveAndSlide();
    }
}
