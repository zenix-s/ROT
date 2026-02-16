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
        ApplyAllMultipliers();
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

    public override void _Process(double delta)
    {
        var gm = GameManager.Instance;
        var prog = gm?.ProgressionManager;
        var arts = gm?.ArtifactManager;

        DebugLabel.Text =
            $"HP: {EntityStatsComponent.CurrentHealth}/{EntityStatsComponent.MaxHealth}\n" +
            $"ATK: {EntityStatsComponent.AttackPower} ({EntityStatsComponent.DamageMultiplier:F2}x)\n" +
            $"Elev: {prog?.CurrentElevation ?? 1} | Arts: {arts?.UsedSlots ?? 0}/{arts?.MaxSlots ?? 1}";
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

    /// <summary>
    ///     Applies stat multipliers from Progression and Artifacts to EntityStatsComponent.
    ///     Call on _Ready and whenever equipment or progression changes.
    /// </summary>
    private void ApplyAllMultipliers()
    {
        var prog = GameManager.Instance?.ProgressionManager;
        var arts = GameManager.Instance?.ArtifactManager;

        float hpMult = prog?.GetHealthMultiplier() ?? 1.0f;
        float dmgMult = prog?.GetDamageMultiplier() ?? 1.0f;

        hpMult += arts?.GetTotalHealthBonus() ?? 0f;
        dmgMult += arts?.GetTotalDamageBonus() ?? 0f;

        EntityStatsComponent.HealthMultiplier = hpMult;
        EntityStatsComponent.DamageMultiplier = dmgMult;
        EntityStatsComponent.RecalculateStats();
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
