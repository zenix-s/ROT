# ROT OF TIME - Game Design Document (Final Scope)

**Date:** 2026-02-15 (actualizado 2026-02-22)
**Status:** FINAL - Shipeable Scope
**Target:** 12-18 months solo development  

---

## Executive Summary

**Rot of Time** is a side-scroller Action-RPG Metroidvania (estilo Blasphemous/Hollow Knight) where you play as a carbon-manipulating mage ascending a mysterious tower. The game focuses on tight combat, meaningful exploration, and build customization through a Charm-like artifact system.

**Core Pillars:**
- ✅ **Focused Combat:** 1 basic attack + 2 customizable spells
- ✅ **Meaningful Progression:** 5 Elevations × 3 Resonances = 15 clear objectives
- ✅ **Build Diversity:** Artifact system (Hollow Knight Charms) for player expression
- ✅ **Metroidvania Exploration:** New abilities unlock backtracking and secrets
- ✅ **Tight Scope:** 8-10 hours, 10 floors, 5 bosses, shippable in 12-18 months

---

## 1. Progression System

### 1.1 Elevaciones (Elevations)

**Structure:**
- 5 Elevations total (major power gates)
- Each Elevation unlocks:
  - New exploration ability
  - 2 new spells to choose from
  - Access to new tower areas
  - +1 Artifact slot (at Elevations 3 and 5)

**Requirements to Advance (at Bonfire):**
1. Tener el item `elevation` en inventario (drop genérico del boss de la Elevación actual)
2. Tener 3 resonancias activadas en la Elevación actual
3. En el Bonfire: consumir el item `elevation` → avanza a la siguiente Elevación

**Item drops:**
- **Resonance trigger** → item `resonance` al inventario (fungible, se activa en Bonfire)
- **Boss defeat** → item `elevation` al inventario (genérico, no específico por elevación)

---

### 1.2 Resonancias (Resonances)

**Mechanics:**
- 3 Resonances per Elevation = **15 total objectives**
- Each Resonance grants:
  - **+20% base HP**
  - **+10% base damage**

**Stat Scaling by Elevation:**

| Elevation | HP Multiplier | Damage Multiplier |
|-----------|---------------|-------------------|
| 1         | 160%          | 130%              |
| 2         | 220%          | 160%              |
| 3         | 280%          | 190%              |
| 4         | 340%          | 220%              |
| 5         | 400%          | 250%              |

**How to Obtain Resonances:**
- Defeat bosses
- Solve exploration puzzles
- Discover secret rooms
- Complete special objectives per floor

**Design Philosophy:**
- No grinding enemies for XP
- Every Resonance is a concrete achievement
- Clear progression tracking for players

---

### 1.3 Exploration Abilities (Metroidvania Gates)

**Elevation 1 - Dash:**
- Basic directional dash
- 1 charge, short cooldown (~1s)
- **Unlocks:** Small gaps, dodge rolls

**Elevation 2 - Infinite Dash:**
- Chain dashes without touching ground (Dash → Dash → Dash)
- Momentum-based movement
- **Unlocks:** Long gaps, speedrun routes, timing-based secrets

**Elevation 3 - Phase Shift:**
- Dash phases through carbon/energy barriers
- Visual: Ethereal/ghostly effect
- **Unlocks:** Secret shortcuts, sealed rooms, hidden areas

**Elevation 4 - Slow-Mo Field:**
- Creates temporal area where projectiles/enemies slow down
- Limited duration, medium cooldown
- **Unlocks:** Timing puzzles, fast traps, tactical combat scenarios

**Elevation 5 - [Narrative Power]:**
- Final ability to confront Ismael Xylos
- Not necessarily an exploration tool

---

## 2. Combat System

### 2.1 Spell Loadout

**Slots:**
- **Basic Attack:** Always equipped, no cooldown
- **Spell Slot 1:** Medium-long cooldown
- **Spell Slot 2:** Medium-long cooldown

**Total Spells in Game:** 8-10
- Unlocked through Elevations (2 spells per Elevation)
- Configurable at Base/Bonfires
- Can swap loadout at any time without cost

---

### 2.2 Spell Examples

**Elevation 1 (Starter):**
- **Carbon Bolt** (Basic Attack): Proyectil rápido, daño bajo
- **Carbon Shell:** Proyectil de carbono denso, mayor daño que Carbon Bolt
- **Carbon Splinter:** Ráfaga de 3 fragmentos de carbono en sucesión rápida

**Elevation 2:**
- **Carbon Nova:** Explosión radial de fragmentos en todas direcciones
- **Shield Dome:** Temporary shield that blocks damage

**Elevation 3:**
- **Chain Lightning:** Hits enemy, chains to nearby targets
- **Carbon Spike:** AoE damage at target location

**Elevation 4:**
- **Homing Missiles:** 5 projectiles that track enemies
- **Gravity Well:** Pulls enemies + continuous damage

**Elevation 5:**
- [Narrative/final spells]

---

## 3. Artifact System (Charm System)

### 3.1 Mechanics

**Artifact Slots:**
- Start with: **1 slot**
- +1 slot at Elevation 3 (total: 2)
- +1 slot at Elevation 5 (total: 3)

**Equipping:**
- Each artifact costs 1-3 slots based on power tier
- Configure at Base/Bonfires without cost
- Can swap freely to experiment with builds

---

### 3.2 Crafting System

**Recipe Structure:**
- Todos los artefactos cuestan solo Isótopos
- No hay recursos secundarios (Fragmentos de Elevación eliminados del diseño)

**Design Philosophy:**
- Simple: un único recurso (Isótopos) para todo el crafteo
- No bloat: no añadir recursos gating que compliquen sin mejorar la experiencia

---

### 3.3 Artifact Catalog

**TIER 1 (1 slot):**

| Artifact | Effect | Cost |
|----------|--------|------|
| Vial de Curación | +1 potion charge | 50 Isótopos |
| Lente de Foco | +15% spell damage | 75 Isótopos |
| Escudo de Grafito | +20% max HP | 75 Isótopos |
| Resonador | +30% Isotope pickup range | 40 Isótopos |

**TIER 2 (2 slots):**

| Artifact | Effect | Cost |
|----------|--------|------|
| Vial Superior | +2 potion charges | 150 Isótopos |
| Corazón Diamantino | 1% HP/sec passive regen | 200 Isótopos |
| Púas de Carbono | Enemies take 20% reflected damage | 150 Isótopos |
| Catalizador | -25% spell cooldowns | 180 Isótopos |

**TIER 3 (3 slots):**

| Artifact | Effect | Cost |
|----------|--------|------|
| Forma Etérea | Dash grants 0.3s invulnerability | 300 Isótopos |
| Anillo de Velocidad | +30% movement speed, -35% cooldowns | 350 Isótopos |

**Total Artifacts in Game:** 10-15

---

## 4. Economy

### 4.1 Isótopos de Carbono

**Sources:**
- Basic enemies: 5-15 per kill
- Chests: 50-200
- Bosses: 500-1000
- Secret rooms: 100-300

**Uses:**
- Craft artifacts (40-350 Isótopos)
- Only resource needed for Tier 1 artifacts

**Balance:**
- No grinding required
- Natural exploration provides sufficient resources
- Players can craft 8-12 artifacts in normal playthrough

---

### 4.2 Inventory Items

**`resonance`**
- Fuente: Resonance triggers repartidos por los floors
- Uso: Consumido en Bonfire para activar una Resonancia (+20% HP, +10% DMG)
- Fungible (acumulable)

**`elevation`**
- Fuente: Drop genérico al derrotar a un boss (no específico por Elevación)
- Uso: Consumido en Bonfire junto a 3 resonancias activadas para avanzar de Elevación
- No acumulable en la práctica (1 boss por Elevación)

> **Decisión de diseño (2026-02-20):** Los items de elevación son genéricos (`elevation`, no `elevation_1/2/3`).
> El crafteo de artefactos usa únicamente Isótopos — no hay recursos especiales de crafteo ligados a Elevaciones.

> **Nota de diseño (2026-02-20):** El crafteo tiene base narrativa sólida — el protagonista proviene de una familia de herreros/alquimistas especializados en crear artefactos. No es una mecánica añadida arbitrariamente; es parte de la identidad del personaje. En el vertical slice el crafteo funciona como tienda simple (solo Isótopos), pero el sistema está diseñado para escalar con Fragmentos de boss y Materiales de Exploración conforme avance el juego.

---

## 5. Healing System

### 5.1 Poción de Carbono

**Base Stats:**
- **Charges:** 3
- **Healing per charge:** 50% max HP
- **Recharge:** At bonfires/checkpoints
- **Input:** [Configurable button]

**Upgrades via Artifacts:**
- Vial de Curación: +1 charge (total 4)
- Vial Superior: +2 charges (total 5)
- Corazón Diamantino: Passive regeneration

**Design Notes:**
- No upgrades to healing "potency"
- Simplicity over complexity
- Player controls healing strategy through equipped artifacts

---

## 6. Content Scope

### 6.1 Tower Structure

**10 Floors Total:**

| Floor | Description | Elevation |
|-------|-------------|-----------|
| 0 | Hub/Base (Order Center) | - |
| 1-2 | Tutorial + initial exploration | 1 |
| 3 | First boss + Xylos revelation | 1 → 2 |
| 4-6 | Interconnected, medium infection | 2-3 |
| 7-9 | Interconnected, high infection | 4-5 |
| 10 | Final boss (Ismael Xylos) | 5 |

---

### 6.2 Bosses

**5 Main Bosses:**
1. Soul Fragment 1 (Floor 3) - Elevation 1 → 2
2. Soul Fragment 2 (Floors 4-6) - Elevation 2 → 3
3. Soul Fragment 3 (Floors 4-6) - Elevation 3 → 4
4. Soul Fragment 4 (Floors 7-9) - Elevation 4 → 5
5. Ismael Xylos (Floor 10) - Final boss

**Optional:** 2-3 mini-bosses for variety

---

### 6.3 Enemies

**8-10 Enemy Types:**
- 2-3 basic types (security robots)
- 2-3 infected types (basic Xylos)
- 2-3 advanced types (elite Xylos)
- Optional: mini-bosses

---

### 6.4 Content Summary

| Category | Count |
|----------|-------|
| Floors | 10 |
| Main Bosses | 5 |
| Enemies | 8-10 types |
| Spells | 8-10 |
| Artifacts | 10-15 |
| **Target Duration** | **8-10 hours** |

**Breakdown:**
- 6-8 hours: Main story
- +2 hours: Secrets/completionist

---

## 7. Technical Architecture

### 7.1 Combat System Architecture

**Capa de datos (Resources):**
- `AttackData` — Resource base: Name, DamageCoefficient, CooldownDuration
- `ProjectileData` — Extiende AttackData: speed, acceleration, lifetime, movementType
- `AttackContext` — Record C# que empaqueta contexto de spawn: direction, position, owner, stats, container

**Jerarquía Skill (Scenes):**
- `Skill` (Node2D abstracto) — base común sin contrato de ejecución
  - `ActiveSkill` (abstracto) — Timer interno, `IsReady`, `GetCooldownProgress()`, `TryExecute(AttackContext)`
    - `ProjectileSkill` [GlobalClass] — exporta `Spawner: AttackSpawnComponent`, llama `Spawner.Execute(ctx)` + `StartCooldown()`
    - `MeleeSkill` (stub abstracto)
  - `PassiveSkill` (stub abstracto) — sin cooldown, gestionado por otro sistema

**Capa de spawn (Components):**
- `AttackSpawnComponent` — Node abstract: `Execute(AttackContext)` orquesta el spawn
  - `SingleSpawnComponent` — Lanza 1 proyectil
  - `BurstSpawnComponent` — Lanza ráfagas (ej: Carbon Splinter × 3)
- `Projectile` (Area2D) — `Initialize(ctx, data)`: conecta hitbox + movimiento
- `AttackMovementComponent` — Node abstract: actualiza `GlobalPosition` en `_PhysicsProcess`
  - `LinearMovementComponent` — Movimiento recto con aceleración

**Manager:**
- `AttackManagerComponent<TSlot>` — Abstract genérica. Mantiene `Dictionary<TSlot, ActiveSkill>`. `TryFire()` crea `AttackContext` y delega a `skill.TryExecute(ctx)`. Cooldowns y timers viven en `ActiveSkill`.
- `PlayerAttackManager` — Concreto. Exporta `PackedScene` por slot (Skills), las registra por `PlayerAttackSlot` enum
- `EnemyAttackManager` — Concreto. Exporta `PackedScene RangedAttackSkill`, registra por `EnemyAttackSlot` enum

**Estructura de un Skill:**
- Cada spell es una scene (ej: `CarbonBoltSkill.tscn`) cuyo nodo raíz es un `ProjectileSkill`
- El `ProjectileSkill` exporta una referencia (`Spawner`) al hijo `AttackSpawnComponent`
- El SpawnComponent tiene referencias a la `ProjectileScene` visual y al `AttackData` Resource

---

### 7.2 Attack Flow

```
Estado del Player → input detectado → PlayerAttackManager.TryFire(slot, dir, pos, stats, owner)
  → AttackManagerComponent crea AttackContext (direction, position, owner, stats, container)
  → ActiveSkill.TryExecute(ctx)
    → verifica IsReady (Timer interno)
    → AttackSpawnComponent.Execute(ctx)
      → Instancia ProjectileScene
      → AddChild a "Main/Attacks"
      → Projectile.Initialize(ctx, data) → hitbox + MovementComponent
    → StartCooldown(duration)
AttackHitboxComponent overlaps HurtboxComponent →
DamageCalculator.Calculate() →
StatsComponent.TakeDamage()
```

---

### 7.3 Attack Manager Structure

```csharp
PlayerAttackManager : AttackManagerComponent<PlayerAttackSlot>
{
    // Cada slot es una PackedScene cuyo nodo raíz es un ActiveSkill (ej: ProjectileSkill)
    [Export] PackedScene BasicAttackSkill;  // CarbonBoltSkill.tscn
    [Export] PackedScene Spell1Skill;       // CarbonShellSkill.tscn
    [Export] PackedScene Spell2Skill;       // CarbonSplinterSkill.tscn
}
```

**Spell Swapping:**
- Done via UI that changes which AttackData is assigned to each slot
- No runtime complexity
- Saves to player save file

---

## 8. Development Priorities

### 8.1 Vertical Slice — COMPLETADO (2026-02-22)

**Logrado:** Combat pipeline, Progression system, Artifact system, Spells (×3), Enemy AI, HUD, Save/Load, BonfireMenu, ArtifactsMenu, Boss SoulFragment1 (código C#).

**Pivote:** Top-down → side-scroller por restricciones de pipeline de arte. Ver Decisions Log en CLAUDE.md (2026-02-22).

**Siguiente:** Transición a side-scroller — física del player, PlayerStateMachine, primer nivel.

---

### 8.2 Alpha (6-9 Months)

- All 5 Elevations
- All 10 floors (grey-box)
- All 5 bosses
- 6-8 enemy types
- Full spell roster (8-10 spells)
- 8-12 artifacts
- Full Metroidvania interconnectivity

---

### 8.3 Beta (9-12 Months)

- Art pass on all floors
- Sound design
- Music
- Polish combat feel
- Balance pass on all systems
- Narrative implementation (notes, dialogues)
- Final ending

---

### 8.4 Ship (12-18 Months)

- Bug fixes
- Performance optimization
- Accessibility options
- Localization (optional)
- Steam page + marketing
- **Launch 1.0**

---

## 9. Success Metrics

**Completion Criteria:**
- Can play from Floor 0 → Floor 10 without bugs
- All 5 bosses defeatable
- All 15 Resonances obtainable
- All spells and artifacts craftable
- Ending implemented
- 8-10 hours of content

**Quality Targets:**
- Combat feels responsive (60 FPS stable)
- Exploration feels rewarding (secrets are satisfying)
- Build diversity feels meaningful (artifacts change playstyle)
- Story is coherent (notes tell Ismael's fall)

**Post-Launch (Optional):**
- Alternate endings
- More artifacts (5-10 additional)
- Challenge modes
- Speedrun support
- Community feedback integration

---

## 10. Risk Mitigation

### 10.1 Scope Creep

**Risk:** Adding features beyond this document  
**Mitigation:** This document is the contract. No additions without cutting something else.

### 10.2 Burnout

**Risk:** 12-18 months solo development is long  
**Mitigation:** 
- Vertical slice validates fun in 3-6 months
- Pivot if slice isn't fun
- Regular playtesting with friends
- Join game dev communities for motivation

### 10.3 Technical Debt

**Risk:** Overengineering or messy code slows development  
**Mitigation:**
- Simplify ruthlessly (already done with combat system)
- Refactor only when blocking progress
- Use Godot's built-in tools (Timers, Resources, Signals)

---

## 11. Why This Will Work

**Compared to original scope:**
- ✅ Cut 50%+ of systems (no mutation, no spell crafting complexity, 1 ending)
- ✅ Clear progression (15 objectives vs vague "complete investigation")
- ✅ Familiar systems (Charms from HK, spell loadouts from ARPGs)
- ✅ Proven combat loop (basic + 2 spells is industry standard)
- ✅ Realistic timeline (12-18 months for 8-10 hours of content)

**What makes it special:**
- ✅ Unique setting (carbon manipulation magic — visual identity fuerte y distintivo)
- ✅ Narrative hook (absorbing corrupted soul fragments)
- ✅ Estética propia (torre industrial-alquímica infectada por Xylos)
- ✅ Build diversity via artifacts (player expression without bloat)

---

## Appendix A: Cut Features (Post-Launch Candidates)

These were removed from core scope but could be added after 1.0 launch:

- Diamond/Graphite mutation system
- Re-attunement cost system
- Spell crafting (projectile + pattern combinations)
- 3 endings (bad, good, true)
- 10 Resonances per Elevation
- Complex research notes system
- Additional artifacts beyond 15
- Extra spells beyond 10
- Challenge modes
- New Game+

**Rule:** Ship 1.0 first. Add these ONLY if the game succeeds.

---

## Appendix B: Core Design Philosophy

**Inspiration:**
- **Hollow Knight:** Tight combat, meaningful exploration, Charm system
- **Dead Cells:** Clear progression, build variety, no grind
- **Hades:** Focused combat loop, player expression through loadouts

**Pillars:**
1. **Respect player time** - No grinding, every objective is meaningful
2. **Clarity over complexity** - Simple systems that interact in interesting ways
3. **Player expression** - Artifacts + spell loadouts create unique builds
4. **Finish over perfection** - Ship a good game, not the perfect game

---

**End of Document**

**Next Steps:**
1. Redactar plan de transición a side-scroller (`docs/plans/`)
2. Implementar física side-scroller (gravedad, salto, player states)
3. Primer nivel side-scroller (grey-box)
4. Refactor boss SoulFragment1 para física platformer
5. Playtest y iterar
6. Ship the game

---

**Document Version:** 1.2
**Last Updated:** 2026-02-22
**Owner:** Zenix (Solo Developer)
**Status:** ✅ APPROVED - Ready for Implementation
