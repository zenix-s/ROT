using System;
using Godot;

public partial class AnimationComponent : Node
{
    [Export] public AnimatedSprite2D AnimatedSprite2D;

    public override void _Ready()
    {
        if (AnimatedSprite2D == null)
            throw new InvalidOperationException("AnimatedSprite2D must be assigned.");
    }

    public void PlayAnimation(string animationName)
    {
        AnimatedSprite2D?.Play(animationName);
    }

    public void StopAnimation(string animationName)
    {
        AnimatedSprite2D?.Stop();
    }

    public bool HasAnimation(string animationName)
    {
        return AnimatedSprite2D?.SpriteFrames?.HasAnimation(animationName) ?? false;
    }

    public void SetModulate(Color color)
    {
        if (AnimatedSprite2D != null)
            AnimatedSprite2D.Modulate = color;
    }
}
