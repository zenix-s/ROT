# Boss Soul Fragment 1 — Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Crear el primer boss del juego (Soul Fragment 1) con 2 fases, dash telegrafíado, daño por contacto y drop de ElevationItem.

**Architecture:** Entidad independiente (`SoulFragment1`) sin base class. StateMachine genérica reutilizada (mismo patrón que BasicEnemy). Body contact via `AttackHitboxComponent` siempre activo. Fase 2 via flag `IsPhase2` que modifica velocidad, intervalo de dash y spread del proyectil.

**Tech Stack:** Godot 4.6, C# (.NET 10.0), StateMachine<T> existente, AttackManagerComponent<TSlot> existente.

**Sin test suite** — verificación: `dotnet build` tras cada tarea de código + testeo manual en Godot (F5) al final.

---

## Archivos a crear

```
Scenes/Bosses/SoulFragment1/
├── SoulFragment1.cs
├── SoulFragment1.tscn            ← creado en Godot (Task 5)
├── BossAttackSlot.cs
├── Components/
│   └── BossAttackManager.cs
└── StateMachine/
    ├── SoulFragmentStateMachine.cs
    └── States/
        ├── IdleState.cs
        ├── ChasingState.cs
        ├── ShootingState.cs
        ├── DashChargingState.cs
        └── DashingState.cs

Resources/Attacks/
├── BossBodyData.tres             ← creado en Godot (Task 5)
└── BossProjectileData.tres       ← creado en Godot (Task 5)

Scenes/Attacks/Skills/
└── BossProjectileSkill.tscn      ← creado en Godot (Task 5)
```

---

## Task 1: BossAttackSlot + BossAttackManager

**Files:**
- Create: `Scenes/Bosses/SoulFragment1/BossAttackSlot.cs`
- Create: `Scenes/Bosses/SoulFragment1/Components/BossAttackManager.cs`

**Step 1: Crear BossAttackSlot**

```csharp
// Scenes/Bosses/SoulFragment1/BossAttackSlot.cs
namespace RotOfTime.Scenes.Bosses.SoulFragment1;

public enum BossAttackSlot
{
    RangedAttack
}
```

**Step 2: Crear BossAttackManager**

Igual que `EnemyAttackManager` pero con `BossAttackSlot`. El namespace es diferente para evitar conflictos.

```csharp
// Scenes/Bosses/SoulFragment1/Components/BossAttackManager.cs
using Godot;
using RotOfTime.Core.Combat.Components;

namespace RotOfTime.Scenes.Bosses.SoulFragment1.Components;

[GlobalClass]
public partial class BossAttackManager : AttackManagerComponent<BossAttackSlot>
{
    [Export] public PackedScene RangedAttackSkill { get; set; }

    public override void _Ready()
    {
        if (RangedAttackSkill != null)
            RegisterSkill(BossAttackSlot.RangedAttack, RangedAttackSkill);
    }
}
```

**Step 3: Verificar que compila**

```bash
dotnet build
```

Esperado: Build succeeded, 0 errors.

**Step 4: Commit**

```bash
git add Scenes/Bosses/SoulFragment1/BossAttackSlot.cs Scenes/Bosses/SoulFragment1/Components/BossAttackManager.cs
git commit -m "feat: añadir BossAttackSlot y BossAttackManager para SoulFragment1"
```

---

## Task 2: SoulFragment1 — Entidad principal

**Files:**
- Create: `Scenes/Bosses/SoulFragment1/SoulFragment1.cs`

**Step 1: Crear SoulFragment1.cs**

```csharp
// Scenes/Bosses/SoulFragment1/SoulFragment1.cs
using System;
using Godot;
using RotOfTime.Core.Combat.Attacks;
using RotOfTime.Core.Combat.Components;
using RotOfTime.Core.Combat.Results;
using RotOfTime.Core.Entities.Components;
using RotOfTime.Core.Progression;
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
```

**Step 2: Verificar que compila**

```bash
dotnet build
```

Esperado: Build succeeded, 0 errors.

**Step 3: Commit**

```bash
git add Scenes/Bosses/SoulFragment1/SoulFragment1.cs
git commit -m "feat: añadir SoulFragment1 entidad base con fases y drop de ElevationItem"
```

---

## Task 3: State Machine + 5 estados

**Files:**
- Create: `Scenes/Bosses/SoulFragment1/StateMachine/SoulFragmentStateMachine.cs`
- Create: `Scenes/Bosses/SoulFragment1/StateMachine/States/IdleState.cs`
- Create: `Scenes/Bosses/SoulFragment1/StateMachine/States/ChasingState.cs`
- Create: `Scenes/Bosses/SoulFragment1/StateMachine/States/ShootingState.cs`
- Create: `Scenes/Bosses/SoulFragment1/StateMachine/States/DashChargingState.cs`
- Create: `Scenes/Bosses/SoulFragment1/StateMachine/States/DashingState.cs`

**Step 1: SoulFragmentStateMachine**

```csharp
// Scenes/Bosses/SoulFragment1/StateMachine/SoulFragmentStateMachine.cs
using RotOfTime.Core.Entities.StateMachine;
using RotOfTime.Scenes.Bosses.SoulFragment1.StateMachine.States;

namespace RotOfTime.Scenes.Bosses.SoulFragment1.StateMachine;

public partial class SoulFragmentStateMachine : StateMachine<SoulFragment1>
{
    public override void _Ready()
    {
        base._Ready();
        ChangeState<IdleState>();
    }
}
```

**Step 2: IdleState**

Espera hasta que hay Target. Los timers NO corren en Idle — se inician al entrar en Chasing.

```csharp
// Scenes/Bosses/SoulFragment1/StateMachine/States/IdleState.cs
using RotOfTime.Core.Entities.StateMachine;

namespace RotOfTime.Scenes.Bosses.SoulFragment1.StateMachine.States;

public partial class IdleState : State<SoulFragment1>
{
    public override void PhysicsProcess(double delta)
    {
        if (TargetEntity.Target != null)
            StateMachine.ChangeState<ChasingState>();
    }
}
```

**Step 3: ChasingState**

Persigue al player. Transiciona a Shooting o DashCharging cuando los timers disparan (comprueba en Process). Si pierde al Target, vuelve a Idle.

```csharp
// Scenes/Bosses/SoulFragment1/StateMachine/States/ChasingState.cs
using Godot;
using RotOfTime.Core.Entities.StateMachine;

namespace RotOfTime.Scenes.Bosses.SoulFragment1.StateMachine.States;

public partial class ChasingState : State<SoulFragment1>
{
    private bool _dashPending;
    private bool _shootPending;

    public override void Enter()
    {
        _dashPending = false;
        _shootPending = false;

        if (TargetEntity.DashTimer != null)
            TargetEntity.DashTimer.Timeout += OnDashTimerFired;
        if (TargetEntity.ShootTimer != null)
            TargetEntity.ShootTimer.Timeout += OnShootTimerFired;
    }

    public override void Exit()
    {
        if (TargetEntity.DashTimer != null)
            TargetEntity.DashTimer.Timeout -= OnDashTimerFired;
        if (TargetEntity.ShootTimer != null)
            TargetEntity.ShootTimer.Timeout -= OnShootTimerFired;
    }

    public override void PhysicsProcess(double delta)
    {
        if (TargetEntity.Target == null)
        {
            StateMachine.ChangeState<IdleState>();
            return;
        }

        // El dash tiene prioridad sobre el disparo
        if (_dashPending)
        {
            _dashPending = false;
            StateMachine.ChangeState<DashChargingState>();
            return;
        }

        if (_shootPending)
        {
            _shootPending = false;
            StateMachine.ChangeState<ShootingState>();
            return;
        }

        var direction = (TargetEntity.Target.GlobalPosition - TargetEntity.GlobalPosition).Normalized();
        TargetEntity.Velocity = direction * TargetEntity.CurrentSpeed;
        TargetEntity.MoveAndSlide();
    }

    private void OnDashTimerFired() => _dashPending = true;
    private void OnShootTimerFired() => _shootPending = true;
}
```

**Step 4: ShootingState**

Se detiene, dispara (en abanico si Fase 2), vuelve a Chasing.

```csharp
// Scenes/Bosses/SoulFragment1/StateMachine/States/ShootingState.cs
using Godot;
using RotOfTime.Core.Entities.StateMachine;

namespace RotOfTime.Scenes.Bosses.SoulFragment1.StateMachine.States;

public partial class ShootingState : State<SoulFragment1>
{
    private bool _fired;

    public override void Enter()
    {
        _fired = false;
        TargetEntity.Velocity = Godot.Vector2.Zero;
    }

    public override void PhysicsProcess(double delta)
    {
        if (_fired)
        {
            StateMachine.ChangeState<ChasingState>();
            return;
        }

        if (TargetEntity.Target == null)
        {
            StateMachine.ChangeState<IdleState>();
            return;
        }

        TargetEntity.MoveAndSlide();

        var direction = (TargetEntity.Target.GlobalPosition - TargetEntity.GlobalPosition).Normalized();
        Fire(direction);
        _fired = true;
    }

    private void Fire(Vector2 direction)
    {
        if (TargetEntity.IsPhase2)
        {
            // Abanico de 3 proyectiles: -20°, 0°, +20°
            float[] angles = { -20f, 0f, 20f };
            foreach (var angle in angles)
                TargetEntity.TryFire(direction.Rotated(Mathf.DegToRad(angle)));
        }
        else
        {
            TargetEntity.TryFire(direction);
        }
    }
}
```

**Step 5: DashChargingState**

Telegraph de `DashChargeDuration` segundos. Captura la dirección al target al entrar. Luego transiciona a Dashing.

```csharp
// Scenes/Bosses/SoulFragment1/StateMachine/States/DashChargingState.cs
using Godot;
using RotOfTime.Core.Entities.StateMachine;

namespace RotOfTime.Scenes.Bosses.SoulFragment1.StateMachine.States;

public partial class DashChargingState : State<SoulFragment1>
{
    [Export] public float ChargeDuration { get; set; } = 0.5f;

    private double _elapsed;

    public override void Enter()
    {
        _elapsed = 0;
        TargetEntity.Velocity = Godot.Vector2.Zero;

        // Captura la dirección en este momento (dash directo al punto actual)
        if (TargetEntity.Target != null)
        {
            TargetEntity.DashDirection =
                (TargetEntity.Target.GlobalPosition - TargetEntity.GlobalPosition).Normalized();
        }
        else
        {
            TargetEntity.DashDirection = Godot.Vector2.Down;
        }
    }

    public override void PhysicsProcess(double delta)
    {
        TargetEntity.MoveAndSlide();
        _elapsed += delta;

        if (_elapsed >= ChargeDuration)
            StateMachine.ChangeState<DashingState>();
    }
}
```

**Step 6: DashingState**

Mueve al boss a `DashSpeed` en la dirección fijada. Termina al recorrer `DashDistance` o al chocar con una pared.

```csharp
// Scenes/Bosses/SoulFragment1/StateMachine/States/DashingState.cs
using RotOfTime.Core.Entities.StateMachine;

namespace RotOfTime.Scenes.Bosses.SoulFragment1.StateMachine.States;

public partial class DashingState : State<SoulFragment1>
{
    private float _traveled;

    public override void Enter()
    {
        _traveled = 0f;
    }

    public override void PhysicsProcess(double delta)
    {
        TargetEntity.Velocity = TargetEntity.DashDirection * TargetEntity.DashSpeed;
        TargetEntity.MoveAndSlide();

        _traveled += TargetEntity.DashSpeed * (float)delta;

        bool hitWall = TargetEntity.GetSlideCollisionCount() > 0;

        if (_traveled >= TargetEntity.DashDistance || hitWall)
            StateMachine.ChangeState<ChasingState>();
    }
}
```

**Step 7: Verificar que compila**

```bash
dotnet build
```

Esperado: Build succeeded, 0 errors.

**Step 8: Commit**

```bash
git add Scenes/Bosses/SoulFragment1/StateMachine/
git commit -m "feat: añadir SoulFragmentStateMachine con 5 estados (Idle/Chasing/Shooting/DashCharging/Dashing)"
```

---

## Task 4: Recursos y Skills en Godot Editor

> Esta tarea es trabajo manual en el editor de Godot.

**Step 1: Crear BossBodyData.tres**

En Godot Editor: `Resources/Attacks/` → New Resource → `AttackData`
- `Name`: "Boss Body"
- `DamageCoefficient`: 1.0
- `CooldownDuration`: 0
- Guardar como `BossBodyData.tres`

**Step 2: Crear BossProjectileData.tres**

En Godot Editor: `Resources/Attacks/` → New Resource → `ProjectileData`
- `Name`: "Boss Projectile"
- `DamageCoefficient`: 1.2
- `CooldownDuration`: 0  ← el ShootTimer controla la cadencia, no el skill
- `InitialSpeed`: 120
- `TargetSpeed`: 120
- `Lifetime`: 5
- `MovementType`: 0 (Linear)
- Guardar como `BossProjectileData.tres`

**Step 3: Crear BossProjectileSkill.tscn**

En Godot Editor: New Scene → Nodo raíz `ProjectileSkill`
- Añadir hijo `SingleSpawnComponent`
  - `ProjectileScene`: `res://Scenes/Attacks/Projectiles/Projectile.tscn`
  - `Data`: `BossProjectileData.tres`
- En `ProjectileSkill` raíz: `Spawner` → apuntar al `SingleSpawnComponent`
- Guardar como `Scenes/Attacks/Skills/BossProjectileSkill.tscn`

---

## Task 5: Ensamblar SoulFragment1.tscn en Godot Editor

> Esta tarea es trabajo manual en el editor de Godot.

**Step 1: Crear la escena base**

New Scene → Nodo raíz `CharacterBody2D`
- Script: `res://Scenes/Bosses/SoulFragment1/SoulFragment1.cs`
- Guardar como `Scenes/Bosses/SoulFragment1/SoulFragment1.tscn`

**Step 2: Añadir hijos (en este orden)**

```
SoulFragment1 (CharacterBody2D) — script: SoulFragment1.cs
├── CollisionShape2D (CapsuleShape2D, size ~20x30)
├── Sprite2D (placeholder: color rojo, texture blanca 32x32)
├── EntityStatsComponent
│    └── EntityStats resource: Vitality=150, Attack=12, Defense=3, Faction=Enemy
├── HurtboxComponent
│    └── CollisionShape2D (CapsuleShape2D, ligeramente más grande)
├── AttackHitboxComponent  ← body damage
│    └── CollisionShape2D (CapsuleShape2D, mismo tamaño que hurtbox)
├── BossAttackManager
│    └── RangedAttackSkill: BossProjectileSkill.tscn
├── DetectionArea (Area2D)
│    └── CollisionShape2D (CircleShape2D, radius=250)
├── DashTimer (Timer, WaitTime=4.0, OneShot=false, Autostart=false)
├── ShootTimer (Timer, WaitTime=2.5, OneShot=false, Autostart=false)
└── SoulFragmentStateMachine
    ├── IdleState
    ├── ChasingState
    ├── ShootingState
    ├── DashChargingState (ChargeDuration=0.5)
    └── DashingState
```

**Step 3: Wiring de exports en SoulFragment1**

Seleccionar el nodo raíz `SoulFragment1` y asignar en el Inspector:
- `EntityStatsComponent` → nodo `EntityStatsComponent`
- `HurtboxComponent` → nodo `HurtboxComponent`
- `BodyHitbox` → nodo `AttackHitboxComponent`
- `AttackManager` → nodo `BossAttackManager`
- `DetectionArea` → nodo `DetectionArea`
- `BodyAttackData` → `BossBodyData.tres`
- `DashTimer` → nodo `DashTimer`
- `ShootTimer` → nodo `ShootTimer`

**Step 4: Physics layers de AttackHitboxComponent (body)**

Seleccionar el `AttackHitboxComponent` hijo:
- La facción se configura vía código en `Initialize()`, pero en el editor verificar que las collision layers no bloquean nada inesperado. El `Initialize()` las sobreescribe en `_Ready()`.

**Step 5: Colocar el boss en la escena de testeo**

Abrir la escena donde tienes al player. Añadir `SoulFragment1.tscn` a la escena.

---

## Task 6: Testeo manual en Godot (F5)

**Checklist de comportamientos:**

- [ ] **Idle → Chasing:** el boss empieza quieto. Al acercarse el player, empieza a perseguir.
- [ ] **Daño por contacto:** tocar al boss reduce HP del player (ver barra de HP en HUD).
- [ ] **Shooting:** el boss se detiene y dispara un proyectil hacia el player cada ~2.5s.
- [ ] **Proyectil daña al player:** el proyectil del boss reduce HP del player al impactar.
- [ ] **DashCharging:** el boss se detiene ~0.5s (telegraph visual mínimo: quieto).
- [ ] **Dash:** el boss se mueve rápido en línea recta, para al llegar a DashDistance o a una pared.
- [ ] **Dash daña por contacto:** si el player está en la trayectoria del dash, recibe daño.
- [ ] **Chasing → Idle:** al alejarse fuera del DetectionArea, el boss para.
- [ ] **Daño al boss:** los spells del player reducen HP del boss.
- [ ] **Phase 2 (50% HP):** al llegar a mitad de vida, el boss dispara abanico × 3 (verificar que salen 3 proyectiles) y el dash es más frecuente.
- [ ] **Muerte:** al llegar a 0 HP, el boss muere y aparece `ElevationItem` en el suelo.
- [ ] **ElevationItem recogible:** recoger el item y usarlo en el Bonfire avanza la Elevación.

---

## Notas de implementación

- **`ChasingState` conecta/desconecta los signals de los timers en `Enter()`/`Exit()`** para evitar que los pendientes persistan al cambiar de estado. Si se deja conectado permanentemente, un timer que dispara mientras el boss está en `DashingState` causaría transición incorrecta.
- **`DashChargingState.ChargeDuration`** es un `[Export]` en el nodo estado — configurable desde el editor sin tocar código.
- **El skill del boss tiene `CooldownDuration = 0`** — el `ShootTimer` en el boss controla la cadencia. Si el cooldown fuera > 0, habría conflicto entre el timer del skill y el timer del boss.
- **`BossAttackManager` tiene `[GlobalClass]`** — esto permite usarlo como script en `.tscn` directamente.
