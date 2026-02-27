using System;
using Godot;
using RotOfTime.Autoload;
using RotOfTime.Core.Animation.Components;
using RotOfTime.Core.Dash;
using RotOfTime.Core.Combat.Results;
using RotOfTime.Core.Entities;
using RotOfTime.Core.Entities.Components;
using RotOfTime.Scenes.Player.Components;
using HurtboxComponent = RotOfTime.Core.Combat.Components.HurtboxComponent;

namespace RotOfTime.Scenes.Player;

/// <summary>
///     Result of a <see cref="Player.TryFireAttack"/> call.
/// </summary>
public enum AttackFireResult
{
    /// <summary>No attack was fired (no input or slot on cooldown).</summary>
    NotFired,

    /// <summary>An instant-cast attack was fired. No state transition needed.</summary>
    FiredInstant,
}

public partial class Player : CharacterBody2D
{
    public const float Speed = 200.0f;

    [Export] public AnimationComponent AnimationComponent;
    [Export] public AnimatedSprite2D Sprite;
    [Export] public PlayerStatsComponent EntityStatsComponent;
    [Export] public EntityInputComponent EntityInputComponent;
    [Export] public EntityMovementComponent EntityMovementComponent;
    [Export] public HurtboxComponent HurtboxComponent;
    [Export] public PlayerAttackManager AttackManager;

    public PlayerAttackSlot? ActiveAttackSlot { get; set; }
    public DashSkill DashSkill { get; private set; }

    public override void _Ready()
    {
        AddToGroup(Groups.Player);
        SetupStatsComponent();
        SetupHurtboxComponent();
        InitializeDashSkill();
    }

    public void UpdateFacing(float horizontalVelocity)
    {
        if (Sprite == null) return;
        if (horizontalVelocity > 0.1f)
            Sprite.FlipH = false;
        else if (horizontalVelocity < -0.1f)
            Sprite.FlipH = true;
    }

    public AttackFireResult TryFireAttack()
    {
        PlayerAttackSlot? slot = GetPressedAttackSlot();
        if (slot == null)
            return AttackFireResult.NotFired;

        Vector2 mousePos = GetGlobalMousePosition();
        Vector2 dir = (mousePos - GlobalPosition).Normalized();
        Vector2 spawnPos = GlobalPosition + dir * 16;

        bool fired = AttackManager.TryFire(
            slot.Value, dir, spawnPos, EntityStatsComponent.EntityStats, this,
            EntityStatsComponent.DamageMultiplier);

        if (!fired)
            return AttackFireResult.NotFired;

        // All attacks are instant (no casting phase)
        return AttackFireResult.FiredInstant;
    }

    private PlayerAttackSlot? GetPressedAttackSlot()
    {
        if (EntityInputComponent.IsAttackJustPressed)
            return PlayerAttackSlot.BasicAttack;

        if (EntityInputComponent.IsSkill1JustPressed)
            return PlayerAttackSlot.Spell1;

        if (EntityInputComponent.IsSkill2JustPressed)
            return PlayerAttackSlot.Spell2;

        return null;
    }

    private void SetupHurtboxComponent()
    {
        if (HurtboxComponent == null)
            throw new InvalidOperationException("HurtboxComponent is not set");

        HurtboxComponent.AttackReceived += OnAttackReceived;
    }

    private void OnAttackReceived(AttackResult attackResult)
    {
        EntityStatsComponent.TakeDamage(attackResult);
        AnimationComponent.Blink(new Color("#FF0000"), 0.5);
        HurtboxComponent.StartInvincibility(0.5f);
    }

    private void SetupStatsComponent()
    {
        if (EntityStatsComponent == null)
            throw new InvalidOperationException("StatsComponent is not set");
        EntityStatsComponent.EntityDied += OnPlayerDied;
        EntityStatsComponent.HealthChanged += OnPlayerHealthChanged;
    }

    private void InitializeDashSkill()
    {
        var type = GameManager.Instance?.DashManager?.Equipped ?? DashType.Standard;
        SpawnDashSkill(type);
    }

    /// <summary>
    ///     Equipa un nuevo dash, swapeando el nodo activo.
    ///     Llamado por DashPanel.
    /// </summary>
    public void EquipDash(DashType type)
    {
        DashSkill?.QueueFree();
        GameManager.Instance?.DashManager?.Equip(type);
        SpawnDashSkill(type);
        GameManager.Instance?.SaveMeta();
    }

    private void SpawnDashSkill(DashType type)
    {
        if (!DashManager.ScenePaths.TryGetValue(type, out string path))
        {
            GD.PrintErr($"Player: DashType '{type}' no tiene scene registrada.");
            return;
        }
        var packed = GD.Load<PackedScene>(path);
        if (packed == null)
        {
            GD.PrintErr($"Player: No se pudo cargar DashSkill en '{path}'");
            return;
        }
        DashSkill = packed.Instantiate<DashSkill>();
        AddChild(DashSkill);
    }

    #region Event Handlers

    private void OnPlayerHealthChanged(int newHealth)
    {
        // TODO: Notify HUD to update health display
    }

    private void OnPlayerDied()
    {
        GameManager.Instance.PlayerDied();
    }

    #endregion
}
