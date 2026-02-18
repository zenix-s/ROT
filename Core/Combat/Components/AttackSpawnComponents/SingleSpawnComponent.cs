using Godot;
using RotOfTime.Core.Combat.Attacks;
using RotOfTime.Scenes.Attacks.Projectiles;

namespace RotOfTime.Core.Combat.Components.AttackSpawnComponents;

[GlobalClass]
public partial class SingleSpawnComponent : AttackSpawnComponent
{
    public override void Execute(AttackContext ctx)
    {
        if (ProjectileScene == null || Data == null)
        {
            GD.PrintErr("SingleSpawnComponent: ProjectileScene or Data is null.");
            return;
        }

        SpawnProjectile(ctx);
        EmitSignal(SignalName.SkillFired, Data.CooldownDuration);
    }

    protected void SpawnProjectile(AttackContext ctx)
    {
        var instance = ProjectileScene.Instantiate<Node2D>();
        instance.GlobalPosition = ctx.SpawnPosition;
        instance.Rotation = ctx.Direction.Angle();

        if (Data is ProjectileData projData)
        {
            var movement = MovementFactory.Create(projData.MovementType);
            instance.AddChild(movement);
        }

        ctx.AttacksContainer.AddChild(instance);

        if (instance is Projectile projectile)
        {
            projectile.Initialize(ctx, Data);
        }
        else
        {
            // Direct hitbox init for scenes without Projectile script (e.g., RockBody)
            foreach (var child in instance.GetChildren())
            {
                if (child is AttackHitboxComponent hitbox)
                {
                    hitbox.Initialize(ctx.OwnerStats, Data, ctx.DamageMultiplier);
                    break;
                }
            }
        }
    }
}
