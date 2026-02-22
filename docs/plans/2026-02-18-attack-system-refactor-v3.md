# Attack System Refactor v3 — Skill + Projectile + Composición por Nodos

**Fecha:** 2026-02-18
**Branch:** `refactor/attack-resource-orchestrator`
**Estado:** Diseño validado, pendiente de implementación

## Problema

El refactor v2 metió lógica de orquestación (spawn, lifecycle, init de componentes) dentro de Resources (`AttackData.Spawn()`, `ProjectileData.Spawn()`, `BurstAttackData.Spawn()`). Esto viola la filosofía de Godot: los Resources son datos, no comportamiento. Perdemos composición por nodos, herencia de scenes, y la capacidad de reusar lógica de forma natural.

## Solución

Tres entidades con responsabilidades claras:

| Entidad | Tipo | Responsabilidad |
|---------|------|----------------|
| **Skill** | Scene (Node2D) | Define CÓMO se ejecuta un ataque: patrón de spawn, qué proyectil, qué datos. Vive como hijo del AttackManager. |
| **Projectile** | Scene (Area2D) + Script base | Ejecutor. Recibe datos, configura sus componentes (hitbox, movement, lifetime), vive y muere. |
| **AttackData** | Resource | Datos puros. Daño, cooldown, lifetime, speed, movement type. Sin lógica. |

## Arquitectura

### Capa 1: AttackManager (modificado)

El `AttackManagerComponent<TSlot>` cambia mínimamente:

- Exports pasan de `AttackData` a `PackedScene` (Skill scenes)
- En `_Ready()` instancia cada Skill como hijo propio y conecta signal `SkillFired`
- `TryFire()` verifica cooldown → llama `skill.Execute(ctx)`
- `OnSkillFired(float cooldown)` arranca el Timer de cooldown

```
PlayerAttackManager : AttackManagerComponent<PlayerAttackSlot>
  [Export] PackedScene BasicAttackSkill  → CarbonBoltSkill.tscn
  [Export] PackedScene Spell1Skill       → FireballSkill.tscn
  [Export] PackedScene Spell2Skill       → IceShardSkill.tscn
```

Las Skills viven como hijos persistentes del Manager. No se instancian/destruyen por disparo.

### Capa 2: Skill Scenes + SpawnComponents

La Skill scene contiene un SpawnComponent que define el patrón de generación.

**Jerarquía de SpawnComponents:**

- `AttackSpawnComponent` (Node, abstracto) — `virtual Execute(AttackContext)`, signal `SkillFired(float cooldown)`
- `SingleSpawnComponent` — Exports: `ProjectileScene`, `AttackData`. Spawna 1 proyectil, emite signal.
- `BurstSpawnComponent` — Exports: `ProjectileScene`, `AttackData`, `BurstCount`, `BurstDelay`. Timer interno para ráfaga.

**Scenes de ejemplo:**

```
CarbonBoltSkill.tscn (Node2D)
└── SingleSpawnComponent (Node)
      [Export] PackedScene ProjectileScene → Projectile.tscn
      [Export] AttackData Data → CarbonBolt.tres

IceShardSkill.tscn (Node2D)
└── BurstSpawnComponent (Node)
      [Export] PackedScene ProjectileScene → Projectile.tscn
      [Export] AttackData Data → IceShardProjectile.tres
      [Export] int BurstCount = 3
      [Export] float BurstDelay = 0.1f
```

**Flujo del SpawnComponent:**

```
Execute(ctx):
  → Instancia ProjectileScene
  → AddChild(MovementFactory.Create(data.MovementType)) al proyectil
  → Añade proyectil a ctx.AttacksContainer (Main/Attacks)
  → projectile.Initialize(ctx, Data)
  → Emite SkillFired(Data.CooldownDuration)
```

El BurstSpawnComponent emite `SkillFired` después del primer proyectil (el cooldown empieza inmediatamente, no al terminar la ráfaga).

### Capa 3: Projectile (script base + herencia)

El Projectile es un "cuerpo vacío" — visual + hitbox. Sin movement propio. Todo viene inyectado.

**Projectile.cs (clase base):**

```
Projectile : Area2D
  [Export] AttackHitboxComponent HitboxComponent

  Initialize(AttackContext ctx, AttackData data):
    → HitboxComponent.Initialize(ctx.OwnerStats, data, ctx.DamageMultiplier)
    → Buscar AttackMovementComponent en hijos (inyectado por SpawnComponent)
    → Si es ProjectileData:
      → movement.Initialize(projectileData, ctx.Direction)
      → Crear Timer(projectileData.Lifetime) → OnLifetimeExpired()
    → Conectar HitboxComponent.AttackConnected → OnImpact()

  virtual OnImpact()          → QueueFree()
  virtual OnLifetimeExpired() → QueueFree()
```

**Projectile.tscn:**

```
Projectile (Area2D + Projectile.cs)
├── Sprite2D
├── CollisionShape2D
└── AttackHitboxComponent (Area2D)
    └── CollisionShape2D
```

**Fireball : Projectile** — override OnImpact() para explosión (futuro).

**RockBody** — sin cambios por ahora. Sin base class melee (YAGNI hasta 2+ melee attacks).

### AttackData — datos puros

```
AttackData : Resource
  [Export] StringName Name
  [Export] float DamageCoefficient = 1.0f
  [Export] float CooldownDuration = 0.5f

ProjectileData : AttackData
  [Export] float InitialSpeed = 300f
  [Export] float TargetSpeed = 300f
  [Export] float Acceleration = 0f
  [Export] float Lifetime = 3f
  [Export] MovementType MovementType = MovementType.Linear
```

Eliminado: `AttackScene`, `Spawn()`, `FindHitbox()`, `FindMovementComponent()`.

### MovementFactory

Registry estático `MovementType → instancia`. YAGNI: usa `new()` directo por ahora, migrará a PackedScene cuando un MovementComponent necesite subnodos (ej. Homing con Area2D).

```
public enum MovementType { Linear }

public static class MovementFactory
{
    public static AttackMovementComponent Create(MovementType type)
    {
        return type switch
        {
            MovementType.Linear => new LinearMovementComponent(),
            _ => new LinearMovementComponent()
        };
    }
}
```

### AttackContext (sin cambios significativos)

```
AttackContext (record):
  Vector2 Direction
  Vector2 SpawnPosition
  Node2D Owner               ← referencia viva para burst tracking
  EntityStats OwnerStats
  float DamageMultiplier
  Node AttacksContainer
```

## Flujo completo: Ice Shard (burst de 3)

```
Player pulsa Key 2
  │
  ▼
PlayerAttackManager.TryFire(Spell2)
  │ ¿cooldown activo? → return false
  │
  ▼
IceShardSkill (hijo del Manager, en el tree)
  │ BurstSpawnComponent.Execute(ctx)
  │
  ├─ Proyectil 1:
  │   → Instantiate Projectile.tscn
  │   → AddChild(MovementFactory.Create(Linear))
  │   → Add to Main/Attacks
  │   → projectile.Initialize(ctx, IceShardProjectile.tres)
  │       → HitboxComponent.Initialize(stats, data, multiplier)
  │       → movement.Initialize(data, direction)
  │       → Timer(lifetime)
  │       → hitbox.AttackConnected → OnImpact()
  │
  ├─ Emite SkillFired(1.2f) → Manager arranca cooldown
  │
  ├─ (0.1s Timer)
  ├─ Proyectil 2: mismo flujo, posición de ctx.Owner (live)
  ├─ (0.1s Timer)
  └─ Proyectil 3: mismo flujo
```

## Cambios por archivo

### Archivos nuevos

| Archivo | Descripción |
|---------|-------------|
| `Core/Combat/Attacks/MovementType.cs` | Enum: `Linear` |
| `Core/Combat/Attacks/MovementFactory.cs` | `Create(MovementType) → AttackMovementComponent` |
| `Core/Combat/Components/AttackSpawnComponents/AttackSpawnComponent.cs` | Base abstracta con signal `SkillFired` |
| `Core/Combat/Components/AttackSpawnComponents/SingleSpawnComponent.cs` | Spawn de 1 proyectil |
| `Core/Combat/Components/AttackSpawnComponents/BurstSpawnComponent.cs` | Spawn de N proyectiles con delay |
| `Scenes/Attacks/Skills/CarbonBoltSkill.tscn` | Skill scene: SingleSpawn + CarbonBolt.tres |
| `Scenes/Attacks/Skills/FireballSkill.tscn` | Skill scene: SingleSpawn + Fireball.tres |
| `Scenes/Attacks/Skills/IceShardSkill.tscn` | Skill scene: BurstSpawn + IceShardProjectile.tres |
| `Scenes/Attacks/Projectiles/Projectile.cs` | Clase base Area2D |

### Archivos modificados

| Archivo | Cambio |
|---------|--------|
| `AttackData.cs` | Eliminar `Spawn()`, `FindHitbox()`, `AttackScene` |
| `ProjectileData.cs` | Eliminar `Spawn()`, `FindMovementComponent()`. Añadir `MovementType` |
| `AttackManagerComponent.cs` | Slots → PackedScene. _Ready() instancia skills como hijos. Signal listener. |
| `PlayerAttackManager.cs` | Exports → PackedScene por slot |
| `Projectile.tscn` | Quitar LinearMovementComponent hijo. Añadir Projectile.cs como script root |
| `FireBall.tscn` | Quitar movement. Añadir Fireball.cs script |
| `.tres` resources | Quitar AttackScene. ProjectileData gana MovementType |

### Archivos eliminados

| Archivo | Razón |
|---------|-------|
| `BurstAttackData.cs` | Reemplazado por BurstSpawnComponent |

## Principios de diseño

1. **Resources = datos, Nodes = comportamiento.** Sin excepciones.
2. **Skill = definición completa.** Contiene qué proyectil, qué datos, qué patrón de spawn.
3. **Projectile = agente obediente.** Solo recibe y actúa. No sabe quién lo creó.
4. **Composición sobre herencia.** Movement, hitbox, spawn pattern — todos son componentes intercambiables.
5. **Factory para movimiento.** `new()` hoy, PackedScene mañana. El contrato no cambia.
6. **Signals para comunicación skill→manager.** Godot-native, desacoplado.
7. **YAGNI.** Sin MeleeAttack base class. Sin MovementType.Homing. Sin SpreadSpawnComponent. Se crean cuando se necesiten.
