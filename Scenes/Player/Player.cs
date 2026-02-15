using System;
using Godot;
using RotOfTime.Autoload;
using RotOfTime.Core.Animation.Components;
using RotOfTime.Core.Combat.Results;
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

    /// <summary>A non-instant-cast attack was fired. Caller should transition to CastingState.</summary>
    FiredNeedsCast,
}

public partial class Player : CharacterBody2D
{
    public const float Speed = 200.0f;

    [Export] public AnimationComponent AnimationComponent;
    [Export] public Label DebugLabel;
    [Export] public EntityStatsComponent EntityStatsComponent;
    [Export] public EntityInputComponent EntityInputComponent;
    [Export] public EntityMovementComponent EntityMovementComponent;
    [Export] public HurtboxComponent HurtboxComponent;
    [Export] public PlayerAttackManager AttackManager;

    public PlayerAttackSlot? ActiveAttackSlot { get; set; }

    public override void _Ready()
    {
        SetupStatsComponent();
        SetupHurtboxComponent();
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
            slot.Value, dir, spawnPos, EntityStatsComponent.EntityStats, this);

        if (!fired)
            return AttackFireResult.NotFired;

        var attackSlot = AttackManager.GetSlot(slot.Value);
        if (attackSlot is { IsInstantCast: false })
        {
            ActiveAttackSlot = slot.Value;
            return AttackFireResult.FiredNeedsCast;
        }

        return AttackFireResult.FiredInstant;
    }

    private PlayerAttackSlot? GetPressedAttackSlot()
    {
        if (EntityInputComponent.IsAttackJustPressed)
            return PlayerAttackSlot.BasicAttack;

        if (EntityInputComponent.IsAbility1JustPressed)
            return PlayerAttackSlot.Ability1;

        if (EntityInputComponent.IsAbility2JustPressed)
            return PlayerAttackSlot.Ability2;

        if (EntityInputComponent.IsAbility3JustPressed)
            return PlayerAttackSlot.Ability3;

        return null;
    }

    public override void _Process(double delta)
    {
        DebugLabel.Text =
            $"Health: {EntityStatsComponent.CurrentHealth}/{EntityStatsComponent.EntityStats.VitalityStat}\n";
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
