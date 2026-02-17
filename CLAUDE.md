# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Rot of Time** is an **Action-RPG Metroidvania** in top-down perspective, built with Godot 4.6 and C# (.NET 10.0). Players embody a carbon-manipulating mage ascending a mysterious tower infected by the Xylos hive-mind. Tight combat with spell loadouts, build diversity via artifacts (Hollow Knight-style Charms), and exploration-based progression (no grinding). Currently in pre-alpha.

**Target:** 8-10 hours gameplay, shippable in 12-18 months (solo dev)

## Agent Directives

### Context
The developer is a solo first-time game developer with strong programming skills but no prior game shipping experience. Communication is in Spanish. All design documents are in Spanish.

### Core Mandate: Scope Guardian
- **Be brutally honest** ("sin paños calientes") about what is realistic vs overengineered for a solo dev on a 12-18 month timeline.
- **Actively flag scope creep.** If the developer proposes a feature, system, or abstraction that risks the timeline, say so directly. Do not agree to avoid conflict.
- **Pragmatism over elegance.** The goal is to ship a playable game, not to build a perfect engine. If a simpler approach gets to "fun" faster, recommend it even if it's less architecturally pure.
- **YAGNI ruthlessly.** Do not build systems for hypothetical future needs. If it's not in the vertical slice plan, it doesn't exist yet.
- **Challenge assumptions.** If a design decision seems driven by "what AAA games do" rather than "what a solo dev can ship," push back.

### How to Operate
- **Treat `docs/plans/2026-02-15-game-design-final.md` as the source of truth** for scope. Anything not in that document needs explicit justification before implementation.
- **Refer to `docs/plans/2026-02-15-vertical-slice-plan.md`** for the current implementation plan. Stay within the current phase.
- **When in doubt, ask.** Better to clarify than to build the wrong thing.
- **Prefer small, testable increments.** Each change should be verifiable in Godot (F5) without requiring other systems to be built first.
- **Document decisions** in the Decisions Log section of this file when making architectural or scope choices.

### What "Critical" Means Here
- If a proposed feature adds more than 2-3 days of work and isn't in the vertical slice plan: flag it.
- If an abstraction has fewer than 2 concrete implementations planned: it's premature. Suggest the concrete version.
- If a system is being designed for 10+ use cases but only 2-3 exist in the game: simplify.
- If the developer is polishing a system instead of building the next missing piece: redirect.

## Game Design Summary

### Combat System
- **Active directional combat** with full aiming control
- **1 basic attack** (Carbon Bolt) - always equipped, cooldown 0.4s
- **2 spell slots** (skill_1, skill_2) - cooldown-based, swappable at base/bonfires
- **8-10 spells total** unlocked through progression
- **All attacks are instant** (no casting phase - removed in refactor)
- **Controls (Keyboard/Mouse):** WASD movement, mouse aim, LMB attack, Space dash, 1-2 for spells
- **Controls (Gamepad):** Left stick move, right stick aim, RT attack, LB dash, face buttons for spells

### Progression System (Elevations & Resonances)
- **5 Elevations** (major power gates)
- **3 Resonances per Elevation** = 15 total objectives
- **Each Resonance grants:** +20% HP, +10% damage
- **Progression:** Complete 3 Resonances → Defeat boss → Unlock next Elevation
- **No grinding:** Resonances from bosses, puzzles, secrets (not enemy farming)
- **Ability gating:** Each Elevation unlocks new exploration ability (Dash, Infinite Dash, Phase Shift, Slow-Mo, etc.)

### Artifact System (Charm System)
- **Artifact Slots:** Start with 1, max 3 (unlock at Elevations 3 & 5)
- **Each artifact costs 1-3 slots** based on power
- **10-15 artifacts total** with effects like:
  - Extra potion charges
  - Increased spell damage
  - Passive health regen
  - Reduced cooldowns
  - Dash invulnerability
- **Crafting:** Isótopos (enemy drops) + optional boss fragments
- **Swappable at base** without cost (encourages experimentation)

### Level Design
- Hand-crafted floors with Metroidvania interconnectivity
- Floor hubs with teleport points (require "Magic Signature Registration")
- Shortcuts (stairs, levers, elevators) connecting floor ends to beginnings
- Backtracking with new abilities to access previously inaccessible areas

## Build & Run Commands

```bash
# Build the project
dotnet build

# Run the game via Godot
godot --path .
# Or open in Godot Editor and press F5
```

**No test suite** - verification is `dotnet build` + manual testing in Godot Editor (F5).

## Solution File

This project uses `.slnx` (XML-based solution format), not `.sln`. Godot regenerates a `.sln` file on each build—this is expected behavior. The `.sln` file is gitignored; always use `RotOfTime.slnx` for IDE configuration.

## Architecture

### Autoload Singletons (`Autoload/`)
Registered in `project.godot` `[autoload]` section:
- **GameManager** (`Instance` property) - Runtime meta-progression state. Creates SaveManager, GameStateManager, AbilityManager, ProgressionManager, and ArtifactManager internally.
- **SceneManager** (`Instance` property) - Scene transitions via signals (`SceneChangeRequested`, `MenuChangeRequested`)

Not autoloads (plain C# classes created by GameManager):
- **SaveManager** - File I/O for persistence (JSON to `user://saves/`)
- **GameStateManager** - Milestone tracking with C# 13 `extension` blocks
- **AbilityManager** - Player ability loadout management (stub)
- **ProgressionManager** - Elevation/Resonance tracking, stat multiplier calculation (plain C# class, not a Godot Node)
- **ArtifactManager** - Artifact ownership/equipment, stat bonus calculation (plain C# class, not a Godot Node)

### Entity System (`Core/Entities/`)
Reusable components attached to any entity (Player, Enemies):
- **EntityStats** - `[Resource]` with VitalityStat, AttackStat, DefenseStat, Faction
- **EntityInputComponent** (Node) - Input polling: Direction, IsAttackJustPressed, IsDashJustPressed, IsSkill1JustPressed, IsSkill2JustPressed
- **EntityMovementComponent** (Node) - Velocity management: Move(), Dash(), KnockBack(), StopMovement()
- **EntityStatsComponent** (Node) - Health management with external multipliers (HealthMultiplier, DamageMultiplier), computed MaxHealth/AttackPower, TakeDamage(AttackResult), RecalculateStats(), signals: HealthChanged, EntityDied, StatsUpdated

### State Machine (`Core/Entities/StateMachine/`)
Generic abstract state machine pattern:
- **IState** - Interface: StateId, Enter(), Exit(), Process(), PhysicsProcess()
- **State\<T\>** - Abstract Node implementing IState, holds TargetEntity and StateMachine refs
- **StateMachine\<T\>** - Auto-registers child State\<T\> nodes, ChangeState\<TState\>(), routes _Process/_PhysicsProcess

Used by: `PlayerStateMachine` (IdleState, MoveState, DashState) and `EnemyStateMachine` (IdleState, ChasingState)

### Attack System (`Core/Combat/`) - 2-Layer Architecture

**Data layer (Resources):**
- **AttackData** - `[Resource]`: Name, DamageCoefficient, CooldownDuration, AttackScene (PackedScene)
- **ProjectileData** - Extends AttackData: InitialSpeed, TargetSpeed, Acceleration, Lifetime
- **AttackResult** - `[Resource]`: RawDamage, AttackName, IsCritical (passed through Godot signals)
- **DamageResult** - C# `record`: RawDamage, FinalDamage, AttackName (internal use only)

**Behavior layer (Scenes implementing IAttack):**
- **IAttack** - Interface: `Execute(Vector2 direction, EntityStats, AttackData, float damageMultiplier)`
- **Projectile** (CharacterBody2D) - Base projectile with movement component and lifetime Timer
- **Fireball** - Extends Projectile, spawns explosion on impact
- **RockBody** (Area2D) - Melee body attack

**Manager:**
- **AttackManagerComponent\<TSlot\>** - Abstract generic Node. Timer-based cooldowns (Godot OneShot Timers). Spawns attack scenes into `Main/Attacks` container.
- **PlayerAttackManager** - Concrete: Exports BasicAttackData, Spell1Data, Spell2Data. Enum: `PlayerAttackSlot { BasicAttack, Spell1, Spell2 }`
- **EnemyAttackSlot** - Enum exists (`{ BodyAttack }`) but not yet wired to a manager

**Important:** `AttackManagerComponent<TSlot>` is abstract generic - it **cannot** be used directly as a Godot node script (will crash .tscn files). Always use a concrete subclass like `PlayerAttackManager`.

### Data Flow: Attack Spawning
```
Player.TryFireAttack(slot)
  → PlayerAttackManager.TryFire(slot, direction, position, stats, ownerNode)
    → AttackManagerComponent.SpawnAttack()
      → Instantiate AttackData.AttackScene as Node2D
      → AddChild to "Main/Attacks" container
      → Cast to IAttack → Execute(direction, ownerStats, attackData)
        → AttackHitboxComponent.Initialize() → DamageCalculator.CalculateRawDamage() → AttackResult
```

### Data Flow: Damage Pipeline
```
AttackHitboxComponent overlaps HurtboxComponent
  → HurtboxComponent emits AttackReceived(AttackResult)
  → EntityStatsComponent.TakeDamage(AttackResult)
    → DamageCalculator.CalculateFinalDamage(AttackResult, defenderStats) → DamageResult
    → CurrentHealth -= FinalDamage
    → Signals: HealthChanged / EntityDied
```

### Hitbox/Hurtbox Components (`Core/Combat/Components/`)
- **AttackHitboxComponent** (Area2D) - Faction-aware collision layers, calculates AttackResult, emits `AttackConnected`
- **HurtboxComponent** (Area2D) - Emits `AttackReceived(AttackResult)`, i-frame support via internal Timer

### Attack Movement (`Core/Combat/Components/AttackMovementComponents/`)
- **AttackMovementComponent** - Abstract Node: `Initialize(ProjectileData)`, abstract `CalculateVelocity()`
- **LinearMovementComponent** - Straight-line with MoveToward acceleration

### Save System (`Core/GameData/`)
- **MetaData** - Permanent progression (completed milestones, current elevation, unlocked resonances, artifact slots/owned/equipped). Serialized to JSON.
- **GameData/PlayerData** - Planned for run state (not yet implemented)

### Scene Management (`Core/SceneExtensionManager.cs`)
Static dictionaries map enums to scene paths:
- `GameScene` enum → `ScenePaths` dictionary
- `MenuScene` enum → `MenuPaths` dictionary
- `TowerLevel` enum → level progression mapping

### Physics Layers
| Layer | Name | Bit |
|-------|------|-----|
| 1 | World | 1 |
| 2 | Player | 2 |
| 3 | Enemies | 4 |
| 6 | PlayerDamageBox | 32 |
| 7 | EnemyDamageBox | 64 |
| 8 | PlayerAttack | 128 |
| 9 | EnemyAttack | 256 |
| 10 | Projectiles | 512 |

## Key Patterns

- **Godot Resources** (`[GlobalClass]` attribute) for data: `EntityStats`, `AttackData`, `ProjectileData`, `AttackResult`
- **C# records** for internal-only data: `DamageResult`
- **Signals** for decoupling: Components emit events, entity scripts subscribe
- **Singleton autoloads** with `Instance` pattern for global access
- **Generic abstract base classes** for shared logic: `AttackManagerComponent<TSlot>`, `StateMachine<T>`, `State<T>`
- **IAttack interface** separates attack data (Resource) from behavior (scene) - scenes have no knowledge of their stats until `Execute()` injects them
- **Timer-based cooldowns** using Godot OneShot Timers (not manual delta tracking)
- **Attacks spawn into** `Main/Attacks` Node2D container (looked up at `GetTree().Root` → `"Main/Attacks"`)

## Important Gotchas

- **ProjectileData inherits from AttackData** - intentional, supports both projectile and non-projectile attacks
- **Input actions use `skill_1` and `skill_2`** (not "spell") - these already existed in the Godot input map
- **Player.tscn `[Export]` NodePath references** require both: adding to `node_paths` PackedStringArray AND adding `NodePath("...")` property in .tscn
- **Abstract generic classes cannot be Godot node scripts** - always use concrete subclass in .tscn files
- **When editing .tscn `ext_resource` lines** be careful not to delete adjacent lines; `ext_resource` IDs like `"11_is"` are referenced by `ExtResource("11_is")` later
- **`Core/Main.tscn`** may be stale - the real main scene is `Scenes/Main/Main.tscn` (referenced by UID in project.godot)

## Current Development Phase

Pre-alpha. Attack system refactor completed. Progression system implemented. Artifact system implemented. Spells implemented. Next: Enemy AI and Combat.

See `docs/plans/2026-02-15-vertical-slice-plan.md` for the full 18-task, 9-phase plan.

**Current status (Vertical Slice):**
- [x] Phase 1: Attack System Refactor
- [x] Phase 2, Tasks 1-2: Progression System (Elevations & Resonances)
- [x] Phase 2, Tasks 3-4: Artifact System (slots, equip/unequip, stat modifiers)
- [x] Phase 3: Spells (Carbon Bolt + Fireball + Ice Shard burst)
- [ ] Phase 4: Enemy AI and Combat ← **SIGUIENTE**
- [ ] Phase 5-6: Level Design + Boss
- [ ] Phase 7-9: UI, Save/Load, Polish

**Donde nos quedamos (2026-02-17):**
- Branch: `feature/vertical-slice`, working tree clean, build passing
- Phase 3 completada y commiteada. Los 3 spells están wired en `Player.tscn` pero **pendientes de testeo manual en Godot (F5)**
- Antes de empezar Phase 4, testear en Godot: LMB (Carbon Bolt), Key 1 (Fireball), Key 2 (Ice Shard burst de 3)
- Phase 4 es Enemy AI (Task 8: Security Robot con chase AI + Task 9: Isotope drops)

## Decisions Log

### 2026-02-15: Attack System Refactor
- **Before:** 5 layers (Player → AttackManager → AttackSlot → SpawnerComponent → IAttack), ~600 lines
- **After:** 2 layers (Player → AttackManagerComponent → IAttack), ~200 lines
- Removed: AttackSlot, AttackSpawnerComponent, SingleShotSpawner, BurstSpawner, TurretSpawner, CastingState
- Removed casting phase entirely (all attacks instant)
- Cooldowns now use Godot Timer nodes (OneShot) instead of manual `_cooldownRemaining` delta tracking
- PlayerAttackManager exports AttackData resources directly (no intermediate AttackSlot nodes)
- Reduced from 4 attack slots to 3 (BasicAttack + 2 Spells)

### 2026-02-16: Progression System Architecture
- **Plan said:** `ProgressionComponent` as Godot Node child of Player, with `ResonanceData` and `ElevationData` Resources
- **Actual:** `ProgressionManager` as plain C# class owned by `GameManager` — progression is global game state, not entity-specific
- **Deleted:** `ResonanceData.cs`, `ElevationData.cs` — YAGNI, all resonances have identical mechanics (+20% HP, +10% DMG), no need for per-resonance Resources
- **Key decision:** `EntityStatsComponent` uses simple `HealthMultiplier`/`DamageMultiplier` float properties (no dependency on GameManager). `Player.cs` acts as coordinator bridging global state to components.
- **Save/load:** `MetaData` extended with `CurrentElevation` and `UnlockedResonances` fields, wired through `GameManager.LoadMeta()`/`SaveMeta()`

### 2026-02-16: Artifact System Architecture
- **Plan said:** `ArtifactManagerComponent` (Godot Node child of Player), `ArtifactSlot` struct, enum `ArtifactEffect` with hardcoded effect types
- **Actual:** `ArtifactManager` as plain C# class owned by `GameManager` (same pattern as ProgressionManager)
- **Simplified effects:** `ArtifactData` Resource with `HealthBonus`/`DamageBonus` floats. No enum/Resource for effects — YAGNI until mechanical effects (potions, regen) have supporting systems
- **Persistence:** Resource paths stored in MetaData, loaded via `GD.Load<ArtifactData>(path)`
- **Coordination:** `Player.ApplyAllMultipliers()` combines progression + artifact bonuses into EntityStatsComponent multipliers
- **3 example artifacts:** Escudo de Grafito (+20% HP, 1 slot), Lente de Foco (+15% DMG, 1 slot), Nucleo Denso (+25% HP +15% DMG, 2 slots)

### 2026-02-16: Spells Implementation
- **Carbon Bolt:** Already existed as `Projectile.tscn` + `CarbonBolt.tres`. No changes needed — was already wired as `BasicAttackData` in `Player.tscn`.
- **Fireball:** Scene and script already existed (`FireBall.tscn`, `Fireball.cs`). Created proper `Resources/Attacks/Fireball.tres` (ProjectileData: speed 200, cooldown 2.0s, lifetime 4s, dmg 1.5x). Old stub `FireballAttackData.tres` left in place.
- **Ice Shard (burst):** Plan said single projectile with slow effect. Actual: **burst of 3 projectiles** fired in rapid sequence (0.1s delay). No slow — YAGNI, no status effect system exists.
- **Key decision:** Burst logic lives in `IceShard.Execute()` (self-contained). If burst pattern is needed again, extract to a spawn strategy component (analogous to `AttackMovementComponent` for movement). Not premature — only 1 user.
- **IceShard architecture:** Node2D implementing IAttack (not a Projectile). Exports `ProjectileScene` + `SubProjectileData`. Spawns into `Main/Attacks` container. Self-destructs after all projectiles fired.
