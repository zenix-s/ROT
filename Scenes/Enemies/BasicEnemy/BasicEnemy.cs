using Godot;

namespace RotOfTime.Scenes.Enemies.BasicEnemy;

/// <summary>
///     Basic enemy that moves toward the player when detected.
/// </summary>
public partial class BasicEnemy : CharacterBody2D
{
    [Export] public float Speed { get; set; } = 50f;
    [Export] public Area2D DetectionArea { get; set; }

    private Node2D _target;

    public override void _Ready()
    {
        if (DetectionArea != null)
        {
            DetectionArea.BodyEntered += OnDetectionAreaBodyEntered;
            DetectionArea.BodyExited += OnDetectionAreaBodyExited;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_target == null) return;

        var direction = (_target.GlobalPosition - GlobalPosition).Normalized();
        Velocity = direction * Speed;
        MoveAndSlide();
    }

    private void OnDetectionAreaBodyEntered(Node2D body)
    {
        GD.Print("Hello");
        if (body is Player.Player player)
            _target = body;
    }

    private void OnDetectionAreaBodyExited(Node2D body)
    {
        if (body == _target)
            _target = null;
    }
}
