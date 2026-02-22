# Boss: Soul Fragment 1 — Diseño

**Fecha:** 2026-02-22
**Fase del vertical slice:** Phase 6, Task 11

---

## Resumen

Boss del Elevation 1. Entidad independiente (`SoulFragment1`), sin base class compartida con `BasicEnemy`. Duplicación deliberada de boilerplate hasta tener 2+ enemigos concretos para guiar una extracción correcta.

---

## Mecánicas

### Daño por contacto (siempre activo)
`AttackHitboxComponent` en el cuerpo del boss, inicializado en `_Ready()`. Activo en todos los estados incluyendo Dashing.

### Proyectil
Usa `EnemyAttackManager` con dos slots:
- `EnemyAttackSlot.RangedAttack` → proyectil único lento (Fase 1)
- `EnemyAttackSlot.BurstAttack` → burst × 3 en abanico (Fase 2)

El boss llama al slot correcto según `IsPhase2`.

### Dash
- `DashTimer` dispara cada X segundos → transición a `DashChargingState`
- Telegraph de 0.5s → captura `DashDirection` en `Enter()`
- `DashingState` mueve al boss a `DashSpeed` durante `DashDistance` píxeles
- El daño viene del `AttackHitboxComponent` ya activo — sin código extra

---

## State Machine

```
Idle → (target detectado) → Chasing
Chasing → (DashTimer dispara) → DashCharging
Chasing → (ShootTimer dispara) → Shooting
DashCharging → (0.5s) → Dashing
Dashing → (distancia recorrida) → Chasing
Shooting → (proyectil disparado) → Chasing
```

**Fase 2** (HP < 50%): no es un estado — es el flag `IsPhase2 = true` que:
- Acorta el intervalo de `DashTimer` × 0.6
- Hace que `ShootingState` use `BurstAttack` en vez de `RangedAttack`
- Aumenta la velocidad de chase a `SpeedPhase2`

---

## Estructura de nodos

```
SoulFragment1 (CharacterBody2D)
├── CollisionShape2D
├── EntityStatsComponent
├── HurtboxComponent
├── AttackHitboxComponent   ← body damage, siempre activo
├── EnemyAttackManager
├── DetectionArea (Area2D)
│   └── CollisionShape2D
├── DashTimer (OneShot: false, autostart: false)
├── ShootTimer (OneShot: false, autostart: false)
└── SoulFragmentStateMachine
    ├── IdleState
    ├── ChasingState
    ├── ShootingState
    ├── DashChargingState
    └── DashingState
```

---

## Propiedades configurables (exports)

```csharp
[Export] float SpeedPhase1 = 80f
[Export] float SpeedPhase2 = 130f
[Export] float DashSpeed = 400f
[Export] float DashDistance = 200f
[Export] float DashChargeDuration = 0.5f
[Export] float DashTimerInterval = 4.0f
[Export] float ShootTimerInterval = 2.5f
```

---

## Drop al morir

Instancia `ElevationItem.tscn` en `GlobalPosition` del boss con `CallDeferred(AddChild)`.
Sin isótopos — el boss no droppea currency.

---

## Recursos necesarios

- `BossAttackData.tres` (ProjectileData lento: speed 120, cooldown 0, dmg 1.2x)
- Reutilizar `BurstSpawnComponent` existente para el slot de burst
- `EnemyBoltSkill.tscn` o similar para el slot ranged del boss
