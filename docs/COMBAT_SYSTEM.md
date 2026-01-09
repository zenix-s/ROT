# Combat System Architecture

## Overview

The combat system is implemented as a **pure C# class library** (`RotOfTime.Combat`) with no Godot dependencies. This enables:
- Unit testing without Godot runtime
- Clean separation of game logic from engine code
- Potential reuse in other projects

## Project Structure

```
RotOfTime.Combat/
├── RotOfTime.Combat.csproj
├── Entities/
│   └── EntityStats.cs           # Mutable stats container
├── Attacks/
│   ├── AttackData.cs            # Attack definition (immutable)
│   └── DamageResult.cs          # Calculation result (immutable)
└── Calculations/
    ├── IDamageCalculator.cs     # Interface for DI/testing
    └── DamageCalculator.cs      # Default implementation
```

---

## Core Types

### EntityStats

Immutable record struct representing an entity's **base stats** (template data).
Current state (health, buffs, etc.) is handled by Godot node-based components.

**Namespace:** `RotOfTime.Combat.Entities`

| Property | Type | Description |
|----------|------|-------------|
| `MaxHealth` | int | Maximum health points |
| `Attack` | int | Base attack power |
| `Defense` | int | Damage reduction |

**Usage:**
```csharp
var baseStats = new EntityStats(MaxHealth: 100, Attack: 15, Defense: 5);
```

**Note:** This is a data template. Godot components will hold instances of this and manage current state (current health, etc.).

---

### AttackData

Immutable record struct describing an attack's base properties.
Godot resources will use instances of this for organizing attacks.

**Namespace:** `RotOfTime.Combat.Attacks`

| Property | Type | Description |
|----------|------|-------------|
| `Name` | string | Attack name (required) |
| `DamageCoefficient` | float | Multiplier for attacker's Attack stat (1.0 = 100%) |

**Usage:**
```csharp
var basicAttack = new AttackData(Name: "Basic Attack", DamageCoefficient: 1.0f);
var heavyAttack = new AttackData(Name: "Heavy Strike", DamageCoefficient: 1.5f);
```

**Note:** Godot resources will wrap these for editor configuration and organization.

---

### DamageResult

Immutable record containing the result of a damage calculation.

**Namespace:** `RotOfTime.Combat.Attacks`

| Property | Type | Description |
|----------|------|-------------|
| `RawDamage` | int | Damage before defense reduction |
| `FinalDamage` | int | Damage after all modifiers |
| `WasCritical` | bool | Whether this was a critical hit (future) |
| `WasBlocked` | bool | Whether damage was fully blocked |
| `DamageReduced` | int | Amount absorbed by defense |

**Static Values:**
```csharp
DamageResult.Blocked  // Zero damage result
DamageResult.None     // Empty/missed result
```

---

### IDamageCalculator

Interface for damage calculation strategies.

**Namespace:** `RotOfTime.Combat.Calculations`

```csharp
public interface IDamageCalculator
{
    DamageResult Calculate(EntityStats attacker, EntityStats defender, AttackData attack);
}
```

---

### DamageCalculator

Default damage calculation implementation.

**Namespace:** `RotOfTime.Combat.Calculations`

**Formula:**
```
RawDamage = Attack * DamageCoefficient
FinalDamage = max(MinimumDamage, RawDamage - Defense)
```

**Properties:**
- `MinimumDamage` - Minimum damage dealt (default: 1)
- `Default` - Static singleton instance

**Usage:**
```csharp
var attackerStats = new EntityStats(MaxHealth: 100, Attack: 20, Defense: 5);
var defenderStats = new EntityStats(MaxHealth: 100, Attack: 10, Defense: 8);
var attack = new AttackData(Name: "Slash", DamageCoefficient: 1.0f);

var result = DamageCalculator.Default.Calculate(attackerStats, defenderStats, attack);

// result.RawDamage = 20 (20 * 1.0)
// result.FinalDamage = 12 (20 - 8)
// result.DamageReduced = 8
```

---

## Integration with Godot

The combat library is referenced by the main Godot project. Integration happens through Godot components that hold EntityStats instances and manage current state.

### Example: Using with a CombatComponent

```csharp
using Godot;
using RotOfTime.Combat.Entities;
using RotOfTime.Combat.Attacks;
using RotOfTime.Combat.Calculations;

public partial class CombatComponent : Node
{
    [Signal] public delegate void HealthChangedEventHandler(int oldValue, int newValue);
    [Signal] public delegate void DiedEventHandler();

    [Export] public int MaxHealth { get; set; } = 100;
    [Export] public int Attack { get; set; } = 10;
    [Export] public int Defense { get; set; } = 5;

    public int CurrentHealth { get; private set; }
    public EntityStats BaseStats { get; private set; }
    public bool IsAlive => CurrentHealth > 0;

    private readonly IDamageCalculator _calculator = DamageCalculator.Default;

    public override void _Ready()
    {
        BaseStats = new EntityStats(MaxHealth, Attack, Defense);
        CurrentHealth = MaxHealth;
    }

    public void TakeDamage(EntityStats attackerStats, AttackData attack)
    {
        var result = _calculator.Calculate(attackerStats, BaseStats, attack);
        int oldHealth = CurrentHealth;
        CurrentHealth = Math.Max(0, CurrentHealth - result.FinalDamage);

        EmitSignal(SignalName.HealthChanged, oldHealth, CurrentHealth);

        if (!IsAlive)
            EmitSignal(SignalName.Died);
    }
}
```

---

## Data Flow

```
┌─────────────┐      ┌────────────┐      ┌──────────────────┐
│ AttackData  │ ───► │            │      │                  │
│ (coeff)     │      │  Damage    │ ───► │  DamageResult    │
├─────────────┤      │ Calculator │      │  (final damage)  │
│ Attacker    │ ───► │            │      │                  │
│ EntityStats │      │            │      └────────┬─────────┘
├─────────────┤      └────────────┘               │
│ Defender    │ ───►                              ▼
│ EntityStats │                         ┌──────────────────┐
└─────────────┘                         │ defender.Apply   │
                                        │ Damage()         │
                                        └──────────────────┘
```

---

## Extensibility

The architecture supports future expansion:

| Feature | Extension Point |
|---------|-----------------|
| Critical hits | Add `CriticalChance` to EntityStats, update calculator |
| Damage types | Add `DamageType` enum to AttackData |
| Elemental resistance | Add resistance dictionary to EntityStats |
| Status effects | New `Effects/` folder with effect classes |

### Future Structure (Example)

```
RotOfTime.Combat/
├── Entities/
│   ├── EntityStats.cs
│   └── Resistances.cs              # Elemental resistances
├── Attacks/
│   ├── AttackData.cs
│   ├── DamageResult.cs
│   └── DamageType.cs               # Physical, Fire, Ice, etc.
├── Calculations/
│   ├── IDamageCalculator.cs
│   ├── DamageCalculator.cs
│   └── ElementalCalculator.cs      # Type-aware calculator
└── Effects/
    ├── IStatusEffect.cs
    ├── BurnEffect.cs
    └── PoisonEffect.cs
```

---

## File Locations

| File | Path |
|------|------|
| Combat Library | `/RotOfTime.Combat/` |
| EntityStats | `/RotOfTime.Combat/Entities/EntityStats.cs` |
| AttackData | `/RotOfTime.Combat/Attacks/AttackData.cs` |
| DamageResult | `/RotOfTime.Combat/Attacks/DamageResult.cs` |
| IDamageCalculator | `/RotOfTime.Combat/Calculations/IDamageCalculator.cs` |
| DamageCalculator | `/RotOfTime.Combat/Calculations/DamageCalculator.cs` |
| This Documentation | `/docs/COMBAT_SYSTEM.md` |
