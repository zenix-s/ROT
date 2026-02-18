using Godot;
using RotOfTime.Core.Combat.Attacks;

namespace RotOfTime.Core.Combat.Components.AttackSpawnComponents;

public abstract partial class AttackSpawnComponent : Node
{
    [Signal]
    public delegate void SkillFiredEventHandler(float cooldown);

    [Export] public PackedScene ProjectileScene { get; set; }
    [Export] public AttackData Data { get; set; }

    public abstract void Execute(AttackContext ctx);
}
