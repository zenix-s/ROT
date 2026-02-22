using System;
using Godot;
using RotOfTime.Core.Combat.Attacks;
using RotOfTime.Core.Combat.Components;
using RotOfTime.Core.Combat.Results;
using RotOfTime.Core.Entities.Components;
using RotOfTime.Scenes.Bosses.SoulFragment1.Components;

namespace RotOfTime.Scenes.Bosses.SoulFragment1;

public partial class SoulFragment1 : CharacterBody2D
{
    // --- Exports ---
    [Export] public EntityStatsComponent EntityStatsComponent { get; set; }
    [Export] public HurtboxComponent HurtboxComponent { get; set; }
    [Export] public AttackHitboxComponent BodyHitbox { get; set; }
    [Export] public BossAttackManager AttackManager { get; set; }
    [Export] public Area2D DetectionArea { get; set; }
    [Export] public AttackData BodyAttackData { get; set; }

    // Timers expuestos para que los estados los lean/reseteen
    [Export] public Timer DashTimer { get; set; }
    [Export] public Timer ShootTimer { get; set; }

    // Parámetros de movimiento y fases (configurables desde el editor)
    [Export] public float SpeedPhase1 { get; set; } = 80f;
    [Export] public float SpeedPhase2 { get; set; } = 130f;
    [Export] public float DashSpeed { get; set; } = 400f;
    [Export] public float DashDistance { get; set; } = 200f;

    // --- Estado runtime ---
    public Node2D Target { get; private set; }
    public bool IsPhase2 { get; private set; }
    public Vector2 DashDirection { get; set; }  // capturado en DashChargingState.Enter()

    public float CurrentSpeed => IsPhase2 ? SpeedPhase2 : SpeedPhase1;

    public override void _Ready()
    {
        SetupStats();
        SetupHurtbox();
        SetupBodyHitbox();
        SetupDetection();
        SetupTimers();
    }

    // --- Métodos públicos para los estados ---

    public void TryFire(Vector2 direction)
    {
        AttackManager?.TryFire(
            BossAttackSlot.RangedAttack,
            direction,
            GlobalPosition,
            EntityStatsComponent.EntityStats,
            this
        );
    }

    // --- Setup privado ---

    private void SetupStats()
    {
        if (EntityStatsComponent == null)
            throw new InvalidOperationException("SoulFragment1: EntityStatsComponent no asignado");

        EntityStatsComponent.EntityDied += OnDied;
        EntityStatsComponent.HealthChanged += OnHealthChanged;
    }

    private void SetupHurtbox()
    {
        if (HurtboxComponent == null)
            throw new InvalidOperationException("SoulFragment1: HurtboxComponent no asignado");

        HurtboxComponent.AttackReceived += OnAttackReceived;
    }

    private void SetupBodyHitbox()
    {
        if (BodyHitbox == null || BodyAttackData == null) return;
        BodyHitbox.Initialize(EntityStatsComponent.EntityStats, BodyAttackData);
    }

    private void SetupDetection()
    {
        if (DetectionArea == null) return;
        DetectionArea.BodyEntered += OnDetectionBodyEntered;
        DetectionArea.BodyExited += OnDetectionBodyExited;
    }

    private void SetupTimers()
    {
        DashTimer?.Start();
        ShootTimer?.Start();
    }

    // --- Handlers de eventos ---

    private void OnAttackReceived(AttackResult result)
    {
        EntityStatsComponent.TakeDamage(result);
    }

    private void OnHealthChanged(int newHealth)
    {
        if (!IsPhase2 && newHealth <= EntityStatsComponent.MaxHealth * 0.5f)
        {
            IsPhase2 = true;
            if (DashTimer != null)
                DashTimer.WaitTime *= 0.6f;
        }
    }

    private void OnDied()
    {
        SpawnElevationItem();
        QueueFree();
    }

    private void SpawnElevationItem()
    {
        var scene = GD.Load<PackedScene>("res://Core/Progression/ElevationItem.tscn");
        if (scene == null)
        {
            GD.PrintErr("SoulFragment1: ElevationItem.tscn no encontrado");
            return;
        }
        var item = scene.Instantiate<Node2D>();
        item.GlobalPosition = GlobalPosition;
        GetParent().CallDeferred(Node.MethodName.AddChild, item);
    }

    private void OnDetectionBodyEntered(Node2D body)
    {
        if (body is Player.Player)
            Target = body;
    }

    private void OnDetectionBodyExited(Node2D body)
    {
        if (body == Target)
            Target = null;
    }
}
