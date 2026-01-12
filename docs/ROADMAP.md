# Rot of Time - Development Roadmap

## Current State Assessment

**Engine**: Godot 4.5 + C# 8.0
**Status**: Pre-alpha with foundational architecture

### What's Implemented
- Basic player movement (WASD + jump)
- Damage calculation system (pure C#, testable)
- Component architecture (StatsComponent, Hitbox, Hurtbox)
- Save system framework (meta progression only - money)
- Scene management & basic menu

### Critical Gaps
- No enemies or spawning system
- No spell/ability system (core mechanic)
- No tower progression
- Combat components not integrated
- No gameplay HUD

---

## Development Roadmap

### Phase 1: Core Loop Foundation
**Goal**: Achieve a playable combat loop

1. **Complete Combat Integration**
    - [x] Connect Hitbox → Hurtbox → DamageCalculator → StatsComponent
    - [x] Fix signal naming (AttackConnoted → AttackConnected)
    - [ ] Add visual feedback (damage numbers, hit flash)
    - [ ] Implement death handling
   

2. **Basic Enemy System**
    - [ ] Create Enemy base class with StatsComponent
    - [ ] Simple AI (chase player, melee attack)
    - [ ] Enemy spawner component
    - [ ] At least 2 enemy types

3. **First Spell Implementation**
    - [ ] Auto-cast projectile system
    - [ ] Basic Grimoire data structure
    - [ ] Spell cooldown/mana system
    - [ ] Projectile collision with enemies

4. **Gameplay HUD**
    - [ ] Health bar display
    - [ ] Mana/spell cooldown indicator
    - [ ] Basic damage feedback

---

### Phase 2: Hub & Navigation
**Goal**: Central hub for run management and progression

1. **Hub Scene**
   - Main hub area (The Sanctuary base layout)
   - Navigation between hub zones
   - Atmospheric environment

2. **Run Management**
   - "Start Run" portal/button
   - Grimoire selection before run
   - Run history/statistics display

3. **Inventory System**
   - Collected items/materials display
   - Grimoire collection view
   - Rune inventory (placeholder for Phase 4)

4. **Basic Progression Display**
   - Currency display (money, materials)
   - Unlocked Grimoires showcase
   - Player stats summary

---

### Phase 3: Tower Progression
**Goal**: Vertical gameplay with increasing difficulty

1. **Room/Floor System**
   - Room templates with spawn points
   - Floor completion conditions (clear all enemies)
   - Floor transitions (portal/stairs)
   - Difficulty scaling per floor

2. **Wave System**
   - Enemy wave spawning within rooms
   - Wave completion rewards
   - Boss room markers

3. **Branching Paths** (from README)
   - Choice between power-up rooms vs resource rooms
   - Basic procedural room selection

4. **Run State Save/Load**
   - Implement GameData.cs and PlayerData.cs
   - Save current floor, health, upgrades
   - Resume interrupted runs

---

### Phase 4: Spell Synergies & Upgrades
**Goal**: Build variety and strategic depth

1. **Spell Modifier System**
   - Spell modifiers (pierce, chain, split, AoE)
   - Elemental types (fire, ice, lightning, arcane)
   - Modifier stacking/combinations

2. **Upgrade System**
   - Post-room upgrade selection UI
   - Upgrade pools per element
   - Upgrade rarity tiers

3. **Multiple Grimoires**
   - 3-5 starting Grimoire archetypes
   - Unique base spell per Grimoire
   - Different upgrade synergies

4. **Elemental Synergies**
   - Status effects (burn, freeze, shock)
   - Combo effects between elements
   - Visual effects for each element

---

### Phase 5: Meta-Progression
**Goal**: Long-term player engagement between runs

1. **Grimoire Evolution**
   - Grimoire experience/mastery system
   - Rarity tiers (Common → Legendary)
   - Unlock new latent abilities

2. **Rune Socketing System**
   - Rune item drops during runs
   - Socket slots in Grimoires
   - Permanent stat bonuses

3. **The Sanctuary (Hub)**
   - Visual hub area between runs
   - Upgrade stations (spell power, defense, etc.)
   - Material/currency spending UI
   - NPC vendors/characters

4. **Persistent Unlocks**
   - Unlock new Grimoires through play
   - Unlock new enemy types
   - Unlock new room variants

---

### Phase 6: Content & Polish
**Goal**: Rich, polished experience

1. **Enemy Variety**
   - 10+ enemy types with unique behaviors
   - Elite enemies with modifiers
   - Boss encounters (every 5-10 floors)

2. **Lore System**
   - Collectible lore items
   - Lore UI/codex
   - Environmental storytelling

3. **Achievement System**
   - Track player accomplishments
   - Unlock cosmetics/Grimoires
   - Challenge runs

4. **Audio & Visual Polish**
   - Sound effects for all actions
   - Background music
   - Particle effects
   - Screen shake, hitstop
   - Death animations

5. **UI Polish**
   - Pause menu
   - Death/game over screen
   - Settings menu
   - Run summary screen

---

## Milestone Targets

| Milestone | Deliverable |
|-----------|-------------|
| **M1: Prototype** | Player can kill enemies with auto-cast spells |
| **M2: Hub & Navigation** | Hub scene with run start, Grimoire selection, basic inventory |
| **M3: Vertical Slice** | Complete 5-floor run with upgrades |
| **M4: Core Loop** | Full run with meta-progression |
| **M5: Content Alpha** | Multiple Grimoires, 10+ enemies, sanctuary upgrades |
| **M6: Beta** | All systems implemented, balancing pass |
| **M7: Release** | Polish, achievements, full content |

---

## Recommended Next Steps

**Immediate priorities** (Phase 1):
1. Fix hitbox/hurtbox signal connection
2. Create a simple enemy that can damage player
3. Implement first auto-cast spell projectile
4. Add health bar HUD

This establishes the core gameplay loop that everything else builds upon.
