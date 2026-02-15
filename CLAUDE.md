# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Rot of Time** is an **Action-RPG Metroidvania** in top-down perspective, built with Godot 4.6 and C# (.NET 10.0). Players embody a carbon-manipulating mage ascending a mysterious tower infected by the Xylos hive-mind. Tight combat with spell loadouts, build diversity via artifacts (Hollow Knight-style Charms), and exploration-based progression (no grinding). Currently in pre-alpha.

**Target:** 8-10 hours gameplay, shippable in 12-18 months (solo dev)

## Game Design Summary

### Combat System
- **Active directional combat** with full aiming control
- **1 basic attack** (Carbon Bolt) - always equipped, no cooldown
- **2 spell slots** - cooldown-based, swappable at base/bonfires
- **8-10 spells total** unlocked through progression
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

## Solution File

This project uses `.slnx` (XML-based solution format), not `.sln`. Godot regenerates a `.sln` file on each build—this is expected behavior. The `.sln` file is gitignored; always use `RotOfTime.slnx` for IDE configuration.

## Architecture

### Autoload Singletons (Godot autoloads)
Three global managers accessed via `Instance` property:
- **GameManager** - Runtime meta-progression state (money operations)
- **SaveManager** - File I/O layer for persistence (JSON to `user://saves/`)
- **SceneManager** - Scene transitions via signals (`SceneChangeRequested`, `MenuChangeRequested`)

### Component Architecture (`Core/Components/`)
Reusable Godot Node components attached to entities:
- **StatsComponent** - Health management, invincibility frames, death signals. Takes `EntityStats` resource.
- **AttackHitboxComponent** (Area2D) - Emits `AttackConneted` when overlapping a hurtbox
- **HurtboxComponent** (Area2D) - Emits `AttackReceived` when hit by a hitbox
- **ComponentManager** - Coordinates components on the Player, wires up signal handlers

### Combat System (`Core/Combat/`)
Pure C# damage calculation (no Godot dependencies, testable):
- **DamageCalculator** - Formula: `max(MinimumDamage, Attack * Coefficient - Defense)`
- **AttackData** - Godot Resource defining attack name and damage coefficient
- **DamageResult** - Record with raw/final damage, blocked status
- **EntityStats** - Godot Resource with MaxHealth, Attack, Defense

### Data Flow: Attack → Damage
```
AttackHitbox overlaps Hurtbox
  → HurtboxComponent emits AttackReceived(AttackData)
  → DamageCalculator.Calculate(attacker.EntityStats, defender.EntityStats, AttackData)
  → StatsComponent.TakeDamage(DamageResult)
  → Health signals emitted (HealthChanged, Died)
```

### Save System (`Core/GameData/`)
- **MetaData** - Permanent progression (money, version). Serialized to JSON.
- **GameData/PlayerData** - Planned for run state (not yet implemented)

### Scene Management (`Core/SceneExtensionManager.cs`)
Static dictionaries map enums to scene paths:
- `GameScene` enum → `ScenePaths` dictionary
- `MenuScene` enum → `MenuPaths` dictionary
- `TowerLevel` enum → level progression mapping

## Key Patterns

- **Godot Resources** (`[GlobalClass]` attribute) for data: `EntityStats`, `AttackData`
- **Signals** for decoupling: Components emit events, managers subscribe
- **Singleton autoloads** with `Instance` pattern for global access
- **Pure C# classes** in `Core/Combat/Calculations/` for testable game logic

## Current Development Phase

Working on Phase 1: Core Loop Foundation. See `docs/ROADMAP.md` for full roadmap.

**Immediate priorities:**
1. Connect Hitbox → Hurtbox → DamageCalculator → StatsComponent pipeline
2. Fix signal naming (`AttackConnoted` should be `AttackConnected`)
3. Create basic enemy with AI and StatsComponent
4. Implement first auto-cast spell projectile
