# Vertical Slice Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Build playable prototype of Floors 1-3 with core combat, progression (2 Elevations, 6 Resonances), 2 bosses, and 3-5 artifacts to validate game feel.

**Architecture:** Godot 4.6 + C#, component-based entity system, resource-driven spell/artifact data, state machine for player control.

**Tech Stack:** Godot 4.6, C# (.NET 10.0), component architecture, resource system

**Timeline:** 3-6 months

**Success Criteria:**
- Player can complete Floors 1-3
- Combat feels responsive and fun
- 2 Elevations unlocked via boss fights
- Resonance system functional (6 resonances grant stat boosts)
- 3-5 artifacts craftable and equippable
- Save/load works

---

## Prerequisites

**IMPORTANT:** Complete `2026-02-15-attack-system-refactor.md` BEFORE starting this vertical slice.

**Reason:** Build on simplified foundation, not overengineered system.

---

## Phase 1: Refactor Attack System (1-2 weeks)

See `docs/plans/2026-02-15-attack-system-refactor.md` for full details.

**Summary:**
- Simplify PlayerAttackSlot to 3 slots (Basic, Spell1, Spell2)
- Remove AttackSlot + SpawnerComponent abstraction
- Use Timer nodes for cooldowns
- Make spells self-contained (IAttack per scene)

**Validation:**
- [x] Basic attack fires correctly
- [x] 2 spell slots functional
- [x] Cooldowns work via Timers
- [x] No console errors

---

## Phase 2: Core Systems Foundation (Week 3-4)

### Task 1: Resonance System - Data Layer ---- COMPLETADO

> **NOTA:** La implementacion difiere del plan original. Ver `CLAUDE.md` Decisions Log 2026-02-16.
> - `ProgressionManager` (clase C# plana en GameManager) en vez de `ProgressionComponent` (Node)
> - Sin `ResonanceData`/`ElevationData` Resources (YAGNI: todas las resonancias son identicas)
> - `EntityStatsComponent` usa `HealthMultiplier`/`DamageMultiplier` simples, Player.cs como coordinador

**Files:**
- Create: `Core/Progression/ResonanceData.cs`
- Create: `Core/Progression/ElevationData.cs`
- Create: `Core/Progression/Components/ProgressionComponent.cs`

**Step 1: Create ResonanceData Resource**

```csharp
// Core/Progression/ResonanceData.cs
using Godot;

namespace RotOfTime.Core.Progression;

[GlobalClass]
public partial class ResonanceData : Resource
{
    [Export] public string ResonanceName { get; set; } = "Unnamed Resonance";
    [Export] public string Description { get; set; } = "";
    [Export] public int ElevationNumber { get; set; } = 1; // 1-5
    [Export] public int ResonanceNumber { get; set; } = 1; // 1-3
    [Export] public float HealthBoostPercent { get; set; } = 20f; // +20% HP
    [Export] public float DamageBoostPercent { get; set; } = 10f; // +10% Damage
}
```

**Step 2: Create ElevationData Resource**

```csharp
// Core/Progression/ElevationData.cs
using Godot;
using Godot.Collections;

namespace RotOfTime.Core.Progression;

[GlobalClass]
public partial class ElevationData : Resource
{
    [Export] public int ElevationNumber { get; set; } = 1;
    [Export] public string ElevationName { get; set; } = "First Elevation";
    [Export] public Array<ResonanceData> Resonances { get; set; } = new();
    [Export] public string BossScenePath { get; set; } = "";
    [Export] public string AbilityUnlocked { get; set; } = "Dash";
}
```

**Step 3: Create ProgressionComponent**

```csharp
// Core/Progression/Components/ProgressionComponent.cs
using Godot;
using System.Collections.Generic;

namespace RotOfTime.Core.Progression.Components;

public partial class ProgressionComponent : Node
{
    [Signal] public delegate void ResonanceUnlockedEventHandler(int elevation, int resonance);
    [Signal] public delegate void ElevationCompletedEventHandler(int elevation);
    
    private HashSet<string> _unlockedResonances = new();
    
    [Export] public int CurrentElevation { get; set; } = 1;
    
    public void UnlockResonance(int elevation, int resonance)
    {
        string key = $"{elevation}_{resonance}";
        if (_unlockedResonances.Add(key))
        {
            EmitSignal(SignalName.ResonanceUnlocked, elevation, resonance);
            CheckElevationComplete(elevation);
        }
    }
    
    public bool IsResonanceUnlocked(int elevation, int resonance)
    {
        return _unlockedResonances.Contains($"{elevation}_{resonance}");
    }
    
    public int GetResonanceCount(int elevation)
    {
        int count = 0;
        for (int i = 1; i <= 3; i++)
        {
            if (IsResonanceUnlocked(elevation, i))
                count++;
        }
        return count;
    }
    
    private void CheckElevationComplete(int elevation)
    {
        if (GetResonanceCount(elevation) >= 3)
        {
            EmitSignal(SignalName.ElevationCompleted, elevation);
        }
    }
    
    public float GetTotalHealthMultiplier()
    {
        int totalResonances = 0;
        for (int e = 1; e <= CurrentElevation; e++)
            totalResonances += GetResonanceCount(e);
        
        return 1.0f + (totalResonances * 0.20f); // Base 100% + 20% per resonance
    }
    
    public float GetTotalDamageMultiplier()
    {
        int totalResonances = 0;
        for (int e = 1; e <= CurrentElevation; e++)
            totalResonances += GetResonanceCount(e);
        
        return 1.0f + (totalResonances * 0.10f); // Base 100% + 10% per resonance
    }
}
```

**Step 4: Test ProgressionComponent**

Create: `tests/Core/Progression/Components/ProgressionComponentTests.cs` (if you have test framework)

Manual test:
1. Add ProgressionComponent to Player scene
2. Call `UnlockResonance(1, 1)` in _Ready()
3. Print `GetTotalHealthMultiplier()` → Should be 1.20
4. Verify signal emits

**Step 5: Commit**

```bash
git add Core/Progression/
git commit -m "feat: add Resonance and Elevation data structures + ProgressionComponent"
```

---

### Task 2: Integrate Progression with EntityStatsComponent ---- COMPLETADO

> **NOTA:** Integrado via `Player.ApplyProgressionMultipliers()` que lee de `ProgressionManager`
> y setea multiplicadores en `EntityStatsComponent`. Sin Export de ProgressionComponent.
> Save/load integrado via `MetaData.CurrentElevation` y `MetaData.UnlockedResonances`.

**Files:**
- Modify: `Core/Entities/Components/EntityStatsComponent.cs`
- Modify: `Scenes/Player/Player.cs`

**Step 1: Add ProgressionComponent reference to EntityStatsComponent**

```csharp
// Core/Entities/Components/EntityStatsComponent.cs
[Export] public ProgressionComponent ProgressionComponent { get; set; }

public int MaxHealth => Mathf.RoundToInt(
    EntityStats.VitalityStat * GetHealthMultiplier()
);

public int AttackPower => Mathf.RoundToInt(
    EntityStats.AttackStat * GetDamageMultiplier()
);

private float GetHealthMultiplier()
{
    return ProgressionComponent?.GetTotalHealthMultiplier() ?? 1.0f;
}

private float GetDamageMultiplier()
{
    return ProgressionComponent?.GetTotalDamageMultiplier() ?? 1.0f;
}
```

**Step 2: Update Player.tscn**

1. Add `ProgressionComponent` node as child of Player
2. Export-link it to EntityStatsComponent

**Step 3: Update debug display**

```csharp
// Scenes/Player/Player.cs
public override void _Process(double delta)
{
    var prog = EntityStatsComponent.ProgressionComponent;
    float hpMult = prog?.GetTotalHealthMultiplier() ?? 1.0f;
    float dmgMult = prog?.GetTotalDamageMultiplier() ?? 1.0f;
    
    DebugLabel.Text = $"HP: {EntityStatsComponent.CurrentHealth}/{EntityStatsComponent.MaxHealth}\n" +
                      $"Mult: {hpMult:F2}x HP, {dmgMult:F2}x DMG\n" +
                      $"Elev: {prog?.CurrentElevation ?? 1}";
}
```

**Step 4: Test progression multipliers**

1. Play game
2. Call `ProgressionComponent.UnlockResonance(1, 1)` from console or test button
3. Verify MaxHealth increases by 20%
4. Unlock 3 resonances → HP should be 160% of base

**Step 5: Commit**

```bash
git add Core/Entities/Components/EntityStatsComponent.cs Scenes/Player/Player.cs
git commit -m "feat: integrate ProgressionComponent with EntityStatsComponent for stat scaling"
```

---

### Task 3: Artifact System - Data Layer

**Files:**
- Create: `Core/Artifacts/ArtifactData.cs`
- Create: `Core/Artifacts/ArtifactSlot.cs`
- Create: `Core/Artifacts/Components/ArtifactManagerComponent.cs`

**Step 1: Create ArtifactData Resource**

```csharp
// Core/Artifacts/ArtifactData.cs
using Godot;

namespace RotOfTime.Core.Artifacts;

public enum ArtifactEffect
{
    ExtraPotionCharge,
    IncreasedSpellDamage,
    IncreasedMaxHealth,
    ReducedCooldowns,
    PassiveRegen
}

[GlobalClass]
public partial class ArtifactData : Resource
{
    [Export] public string ArtifactName { get; set; } = "Unnamed Artifact";
    [Export(PropertyHint.MultilineText)] public string Description { get; set; } = "";
    [Export] public int SlotCost { get; set; } = 1; // 1-3
    [Export] public ArtifactEffect EffectType { get; set; }
    [Export] public float EffectValue { get; set; } = 0f;
    
    // Crafting cost
    [Export] public int IsotopeCost { get; set; } = 50;
}
```

**Step 2: Create ArtifactSlot struct**

```csharp
// Core/Artifacts/ArtifactSlot.cs
namespace RotOfTime.Core.Artifacts;

public struct ArtifactSlot
{
    public ArtifactData Artifact { get; set; }
    public bool IsEquipped { get; set; }
}
```

**Step 3: Create ArtifactManagerComponent**

```csharp
// Core/Artifacts/Components/ArtifactManagerComponent.cs
using Godot;
using System.Collections.Generic;

namespace RotOfTime.Core.Artifacts.Components;

public partial class ArtifactManagerComponent : Node
{
    [Signal] public delegate void ArtifactEquippedEventHandler(ArtifactData artifact);
    [Signal] public delegate void ArtifactUnequippedEventHandler(ArtifactData artifact);
    
    private List<ArtifactData> _equippedArtifacts = new();
    private List<ArtifactData> _ownedArtifacts = new();
    
    [Export] public int MaxSlots { get; set; } = 1; // Starts at 1, increases to 3
    
    public int UsedSlots
    {
        get
        {
            int total = 0;
            foreach (var artifact in _equippedArtifacts)
                total += artifact.SlotCost;
            return total;
        }
    }
    
    public bool CanEquip(ArtifactData artifact)
    {
        return UsedSlots + artifact.SlotCost <= MaxSlots;
    }
    
    public bool EquipArtifact(ArtifactData artifact)
    {
        if (!CanEquip(artifact))
            return false;
        
        _equippedArtifacts.Add(artifact);
        EmitSignal(SignalName.ArtifactEquipped, artifact);
        return true;
    }
    
    public void UnequipArtifact(ArtifactData artifact)
    {
        if (_equippedArtifacts.Remove(artifact))
        {
            EmitSignal(SignalName.ArtifactUnequipped, artifact);
        }
    }
    
    public float GetEffectTotal(ArtifactEffect effectType)
    {
        float total = 0f;
        foreach (var artifact in _equippedArtifacts)
        {
            if (artifact.EffectType == effectType)
                total += artifact.EffectValue;
        }
        return total;
    }
    
    public void AddOwnedArtifact(ArtifactData artifact)
    {
        if (!_ownedArtifacts.Contains(artifact))
            _ownedArtifacts.Add(artifact);
    }
    
    public List<ArtifactData> GetOwnedArtifacts() => new(_ownedArtifacts);
    public List<ArtifactData> GetEquippedArtifacts() => new(_equippedArtifacts);
}
```

**Step 4: Test ArtifactManagerComponent**

Manual test:
1. Add ArtifactManagerComponent to Player
2. Create test ArtifactData resource in Godot (IncreasedSpellDamage, +15%, 1 slot, 50 isotopes)
3. Call `EquipArtifact(testArtifact)` → Should succeed
4. Call `GetEffectTotal(IncreasedSpellDamage)` → Should return 15.0
5. Try equipping 2 slot artifact → Should fail (MaxSlots = 1)

**Step 5: Commit**

```bash
git add Core/Artifacts/
git commit -m "feat: add Artifact data structures and ArtifactManagerComponent"
```

---

### Task 4: Create Test Artifact Resources

**Files:**
- Create: `Resources/Artifacts/VialDeCuracion.tres`
- Create: `Resources/Artifacts/LenteDeFoco.tres`
- Create: `Resources/Artifacts/EscudoDeGrafito.tres`

**Step 1: Create Vial de Curación**

In Godot Editor:
1. Right-click `Resources/Artifacts/` → New Resource → ArtifactData
2. Set properties:
   - Name: "Vial de Curación"
   - Description: "+1 carga de poción"
   - SlotCost: 1
   - EffectType: ExtraPotionCharge
   - EffectValue: 1
   - IsotopeCost: 50
3. Save as `VialDeCuracion.tres`

**Step 2: Create Lente de Foco**

1. New ArtifactData
2. Properties:
   - Name: "Lente de Foco"
   - Description: "+15% daño de hechizos"
   - SlotCost: 1
   - EffectType: IncreasedSpellDamage
   - EffectValue: 15
   - IsotopeCost: 75
3. Save as `LenteDeFoco.tres`

**Step 3: Create Escudo de Grafito**

1. New ArtifactData
2. Properties:
   - Name: "Escudo de Grafito"
   - Description: "+20% vida máxima"
   - SlotCost: 1
   - EffectType: IncreasedMaxHealth
   - EffectValue: 20
   - IsotopeCost: 75
3. Save as `EscudoDeGrafito.tres`

**Step 4: Test loading artifacts**

```csharp
// In Player._Ready() temporarily
var vial = GD.Load<ArtifactData>("res://Resources/Artifacts/VialDeCuracion.tres");
GD.Print($"Loaded: {vial.ArtifactName}, Cost: {vial.SlotCost} slots");
```

**Step 5: Commit**

```bash
git add Resources/Artifacts/
git commit -m "feat: add 3 base artifact resources (Vial, Lente, Escudo)"
```

---

## Phase 3: Spells Implementation (Week 5-6)

### Task 5: Create Basic Attack (Carbon Bolt)

**Files:**
- Create: `Scenes/Attacks/Spells/CarbonBolt/CarbonBolt.tscn`
- Create: `Scenes/Attacks/Spells/CarbonBolt/CarbonBolt.cs`
- Create: `Resources/Attacks/CarbonBoltData.tres`

**Step 1: Create CarbonBolt scene**

1. Create new Scene → Node2D
2. Add children:
   - Sprite2D (placeholder: white 8x8 circle)
   - AttackHitboxComponent (Area2D)
   - CollisionShape2D (small circle)
3. Save as `CarbonBolt.tscn`

**Step 2: Implement CarbonBolt script**

```csharp
// Scenes/Attacks/Spells/CarbonBolt/CarbonBolt.cs
using Godot;
using RotOfTime.Core.Combat.Attacks;
using RotOfTime.Core.Combat.Components;
using RotOfTime.Core.Entities;

namespace RotOfTime.Scenes.Attacks.Spells;

public partial class CarbonBolt : Node2D, IAttack
{
    [Export] public AttackHitboxComponent Hitbox { get; set; }
    [Export] public float Speed { get; set; } = 400f;
    [Export] public float Lifetime { get; set; } = 3f;
    
    private Vector2 _direction;
    private Timer _lifetimeTimer;
    
    public override void _Ready()
    {
        _lifetimeTimer = new Timer();
        _lifetimeTimer.WaitTime = Lifetime;
        _lifetimeTimer.OneShot = true;
        _lifetimeTimer.Timeout += () => QueueFree();
        AddChild(_lifetimeTimer);
        
        Hitbox.AttackConnected += () => QueueFree();
    }
    
    public void Execute(Vector2 direction, EntityStats ownerStats, AttackData attackData, float damageMultiplier = 1.0f)
    {
        _direction = direction.Normalized();
        Rotation = _direction.Angle();
        
        Hitbox?.Initialize(ownerStats, attackData, damageMultiplier);
        _lifetimeTimer.Start();
    }
    
    public override void _Process(double delta)
    {
        GlobalPosition += _direction * Speed * (float)delta;
    }
}
```

**Step 3: Create AttackData resource**

In Godot Editor:
1. New Resource → AttackData
2. Properties:
   - Name: "Carbon Bolt"
   - DamageCoefficient: 1.0
   - CooldownDuration: 0.5
   - AttackScene: Load `CarbonBolt.tscn`
3. Save as `Resources/Attacks/CarbonBoltData.tres`

**Step 4: Assign to Player**

1. Open `Player.tscn`
2. Select `PlayerAttackManager`
3. Export property `BasicAttackData` → Load `CarbonBoltData.tres`

**Step 5: Test**

1. Play game
2. Click mouse → Carbon Bolt should fire
3. Verify:
   - Moves in mouse direction
   - Disappears after 3s or on hit
   - Cooldown prevents spam

**Step 6: Commit**

```bash
git add Scenes/Attacks/Spells/CarbonBolt/ Resources/Attacks/CarbonBoltData.tres
git commit -m "feat: implement Carbon Bolt (basic attack)"
```

---

### Task 6: Create Spell 1 (Fireball)

**Files:**
- Create: `Scenes/Attacks/Spells/Fireball/Fireball.tscn`
- Create: `Scenes/Attacks/Spells/Fireball/Fireball.cs`
- Create: `Resources/Attacks/FireballData.tres`

**Step 1-6:** Similar to Carbon Bolt, but:
- Slower speed (200)
- Longer cooldown (2.0s)
- Higher damage coefficient (1.5)
- Add explosion effect on impact (optional: spawn particles, play sound)

**Commit:**
```bash
git commit -m "feat: implement Fireball spell (Spell1 slot)"
```

---

### Task 7: Create Spell 2 (Ice Shard)

**Files:**
- Create: `Scenes/Attacks/Spells/IceShard/IceShard.tscn`
- Create: `Scenes/Attacks/Spells/IceShard/IceShard.cs`
- Create: `Resources/Attacks/IceShardData.tres`

**Step 1-6:** Similar to Carbon Bolt, but:
- Medium speed (300)
- Medium cooldown (1.5s)
- Damage coefficient (1.2)
- Apply slow effect to enemy on hit (optional: implement status effect system)

**Commit:**
```bash
git commit -m "feat: implement Ice Shard spell (Spell2 slot)"
```

---

## Phase 4: Enemy AI and Combat (Week 7-8)

### Task 8: Create Basic Enemy (Security Robot)

**Files:**
- Create: `Scenes/Enemies/SecurityRobot/SecurityRobot.tscn`
- Create: `Scenes/Enemies/SecurityRobot/SecurityRobot.cs`
- Create: `Resources/Enemies/SecurityRobotStats.tres`

**Step 1: Create enemy scene**

1. New Scene → CharacterBody2D
2. Add components:
   - AnimatedSprite2D (placeholder sprite)
   - CollisionShape2D
   - EntityStatsComponent
   - HurtboxComponent
   - Navigation components (for pathfinding)

**Step 2: Implement basic AI**

```csharp
// Scenes/Enemies/SecurityRobot/SecurityRobot.cs
using Godot;
using RotOfTime.Core.Entities.Components;
using RotOfTime.Core.Combat.Components;

namespace RotOfTime.Scenes.Enemies;

public partial class SecurityRobot : CharacterBody2D
{
    [Export] public float Speed { get; set; } = 100f;
    [Export] public float DetectionRange { get; set; } = 200f;
    [Export] public EntityStatsComponent StatsComponent { get; set; }
    [Export] public HurtboxComponent Hurtbox { get; set; }
    
    private Node2D _player;
    
    public override void _Ready()
    {
        _player = GetTree().GetFirstNodeInGroup("Player") as Node2D;
        StatsComponent.EntityDied += OnDied;
        Hurtbox.AttackReceived += OnAttackReceived;
    }
    
    public override void _PhysicsProcess(double delta)
    {
        if (_player == null) return;
        
        float distanceToPlayer = GlobalPosition.DistanceTo(_player.GlobalPosition);
        
        if (distanceToPlayer < DetectionRange)
        {
            Vector2 direction = (_player.GlobalPosition - GlobalPosition).Normalized();
            Velocity = direction * Speed;
            MoveAndSlide();
        }
    }
    
    private void OnAttackReceived(AttackResult result)
    {
        StatsComponent.TakeDamage(result);
        // TODO: Blink effect, play hit sound
    }
    
    private void OnDied()
    {
        // TODO: Drop isotopes, play death animation
        QueueFree();
    }
}
```

**Step 3: Create EntityStats resource**

1. New Resource → EntityStats
2. Properties:
   - VitalityStat: 30
   - AttackStat: 5
   - DefenseStat: 2
3. Save as `Resources/Enemies/SecurityRobotStats.tres`

**Step 4: Test enemy**

1. Place SecurityRobot in test scene
2. Enemy should chase player when in range
3. Enemy takes damage from spells
4. Enemy dies and despawns

**Step 5: Commit**

```bash
git add Scenes/Enemies/SecurityRobot/ Resources/Enemies/
git commit -m "feat: implement basic enemy (SecurityRobot) with chase AI"
```

---

### Task 9: Isotope Drop System

**Files:**
- Create: `Core/Economy/IsotopePickup.tscn`
- Create: `Core/Economy/IsotopePickup.cs`
- Create: `Autoload/EconomyManager.cs`
- Modify: `Scenes/Enemies/SecurityRobot/SecurityRobot.cs`

**Step 1: Create IsotopePickup scene**

```csharp
// Core/Economy/IsotopePickup.cs
using Godot;

namespace RotOfTime.Core.Economy;

public partial class IsotopePickup : Area2D
{
    [Export] public int IsotopeAmount { get; set; } = 10;
    
    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }
    
    private void OnBodyEntered(Node2D body)
    {
        if (body.IsInGroup("Player"))
        {
            EconomyManager.Instance.AddIsotopes(IsotopeAmount);
            QueueFree();
        }
    }
}
```

**Step 2: Create EconomyManager autoload**

```csharp
// Autoload/EconomyManager.cs
using Godot;

namespace RotOfTime.Autoload;

public partial class EconomyManager : Node
{
    public static EconomyManager Instance { get; private set; }
    
    [Signal] public delegate void IsotopesChangedEventHandler(int newAmount);
    
    private int _isotopes = 0;
    
    public int Isotopes
    {
        get => _isotopes;
        private set
        {
            _isotopes = value;
            EmitSignal(SignalName.IsotopesChanged, _isotopes);
        }
    }
    
    public override void _Ready()
    {
        Instance = this;
    }
    
    public void AddIsotopes(int amount)
    {
        Isotopes += amount;
        GD.Print($"Isotopes: {Isotopes}");
    }
    
    public bool SpendIsotopes(int amount)
    {
        if (_isotopes >= amount)
        {
            Isotopes -= amount;
            return true;
        }
        return false;
    }
}
```

**Step 3: Add to Godot autoloads**

Project Settings → Autoload → Add `Autoload/EconomyManager.cs`

**Step 4: Modify enemy to drop isotopes**

```csharp
// SecurityRobot.cs OnDied()
private void OnDied()
{
    // Spawn isotope pickup
    var pickup = GD.Load<PackedScene>("res://Core/Economy/IsotopePickup.tscn").Instantiate<IsotopePickup>();
    pickup.GlobalPosition = GlobalPosition;
    pickup.IsotopeAmount = 10;
    GetParent().AddChild(pickup);
    
    QueueFree();
}
```

**Step 5: Test**

1. Kill enemy
2. Isotope pickup spawns
3. Walk over it → Isotopes increase
4. Verify EconomyManager.Isotopes updates

**Step 6: Commit**

```bash
git add Core/Economy/ Autoload/EconomyManager.cs Scenes/Enemies/SecurityRobot/SecurityRobot.cs
git commit -m "feat: add isotope drop system + EconomyManager autoload"
```

---

## Phase 5: Level Design - Floor 1 (Week 9-10)

### Task 10: Create Floor 1 Scene

**Files:**
- Create: `Scenes/Levels/Floor1/Floor1.tscn`
- Create: `Scenes/Levels/Floor1/Floor1.cs`

**Step 1: Create base scene**

1. New Scene → Node2D
2. Add TileMap for floor layout (grey-box for now)
3. Add collision shapes for walls
4. Add spawn point for player
5. Add 3-5 enemy spawns (SecurityRobot instances)

**Step 2: Add secret area**

1. Create locked door/barrier
2. Add Resonance trigger (Area2D that unlocks Resonance 1-1)
3. Hidden behind breakable wall or requires dash to reach

**Step 3: Create Resonance trigger**

```csharp
// In Floor1.cs or separate component
[Export] public int ElevationNumber { get; set; } = 1;
[Export] public int ResonanceNumber { get; set; } = 1;

private void OnResonanceTriggerEntered(Node2D body)
{
    if (body.IsInGroup("Player"))
    {
        var player = body as Player;
        player.ProgressionComponent.UnlockResonance(ElevationNumber, ResonanceNumber);
        GD.Print($"Unlocked Resonance {ElevationNumber}-{ResonanceNumber}!");
        // Show UI notification
        QueueFree(); // Remove trigger
    }
}
```

**Step 4: Test Floor 1**

1. Spawn player in Floor1
2. Fight enemies
3. Collect isotopes
4. Find secret area → Unlock Resonance 1-1
5. Verify stat boost applies

**Step 5: Commit**

```bash
git add Scenes/Levels/Floor1/
git commit -m "feat: create Floor 1 with enemies and Resonance trigger"
```

---

## Phase 6: Boss Fight - Elevation 1 Boss (Week 11-12)

### Task 11: Create First Boss (Soul Fragment 1)

**Files:**
- Create: `Scenes/Bosses/SoulFragment1/SoulFragment1.tscn`
- Create: `Scenes/Bosses/SoulFragment1/SoulFragment1.cs`
- Create: `Scenes/Bosses/SoulFragment1/States/` (Boss state machine)

**Step 1: Design boss mechanics**

Simple pattern:
- Phase 1: Melee chase + occasional projectile
- Phase 2 (50% HP): Faster, shoots burst projectiles
- Victory: Unlock Elevation 2

**Step 2: Implement boss scene**

(Similar structure to enemy, but with state machine for phases)

**Step 3: Create boss arena**

- Separate scene for boss room
- Locked door until boss defeated
- Trigger that starts boss fight

**Step 4: Test boss**

- Boss phases work
- Defeating boss unlocks Elevation 2
- Player receives notification

**Step 5: Commit**

```bash
git add Scenes/Bosses/SoulFragment1/
git commit -m "feat: implement Elevation 1 boss (Soul Fragment 1)"
```

---

## Phase 7: UI and Menus (Week 13-14)

### Task 12: Create HUD

**Files:**
- Create: `Scenes/UI/HUD/HUD.tscn`
- Create: `Scenes/UI/HUD/HUD.cs`

**Elements:**
- Health bar
- Isotope counter
- Spell cooldown indicators
- Resonance progress (optional)

**Commit:**
```bash
git commit -m "feat: create HUD with health, isotopes, and spell cooldowns"
```

---

### Task 13: Create Artifact Menu

**Files:**
- Create: `Scenes/UI/Menus/ArtifactMenu.tscn`
- Create: `Scenes/UI/Menus/ArtifactMenu.cs`

**Features:**
- Show owned artifacts
- Equip/unequip artifacts
- Display slot usage (X/3)
- Craft new artifacts (spend isotopes)

**Commit:**
```bash
git commit -m "feat: create artifact menu for equipping and crafting"
```

---

## Phase 8: Save/Load System (Week 15)

### Task 14: Implement Save/Load

**Files:**
- Modify: `Autoload/SaveManager.cs` (already exists)
- Create: `Core/GameData/VerticalSliceData.cs`

**Step 1: Define save data**

```csharp
// Core/GameData/VerticalSliceData.cs
using System.Collections.Generic;

namespace RotOfTime.Core.GameData;

public class VerticalSliceData
{
    public int CurrentElevation { get; set; } = 1;
    public List<string> UnlockedResonances { get; set; } = new();
    public int Isotopes { get; set; } = 0;
    public List<string> OwnedArtifacts { get; set; } = new();
    public List<string> EquippedArtifacts { get; set; } = new();
    public int CurrentHealth { get; set; } = 100;
    public string CurrentFloor { get; set; } = "Floor1";
}
```

**Step 2: Implement Save()**

**Step 3: Implement Load()**

**Step 4: Test save/load**

**Commit:**
```bash
git commit -m "feat: implement save/load for vertical slice data"
```

---

## Phase 9: Polish and Testing (Week 16-18)

### Task 15: Playtest and Balance

- Test full Floor 1-3 loop
- Balance enemy health/damage
- Balance spell cooldowns
- Adjust artifact costs
- Fix bugs

### Task 16: Audio (Optional)

- Add basic hit sounds
- Add spell cast sounds
- Add background music

### Task 17: Particle Effects (Optional)

- Spell impact effects
- Enemy death particles
- Isotope pickup sparkles

---

## Success Criteria Checklist

**Core Loop:**
- [ ] Player can move and dash
- [ ] Basic attack and 2 spells work
- [ ] Spells have cooldowns
- [ ] Enemies spawn and chase player
- [ ] Enemies drop isotopes on death

**Progression:**
- [ ] Resonances unlock and grant stat boosts
- [ ] 6 resonances available (3 per Elevation)
- [ ] Boss fight unlocks Elevation 2
- [ ] Stats scale correctly (check with debug display)

**Artifacts:**
- [ ] 3-5 artifacts craftable
- [ ] Artifacts equip/unequip in menu
- [ ] Artifact effects apply (test each)
- [ ] Slot limit enforced (1 slot base)

**Level Design:**
- [ ] Floor 1 explorable
- [ ] Floor 2 explorable
- [ ] Floor 3 + boss arena complete
- [ ] Secret areas reward exploration

**UI:**
- [ ] HUD shows health, isotopes, cooldowns
- [ ] Artifact menu functional
- [ ] Pause menu works

**Technical:**
- [ ] Save/load works
- [ ] No console errors
- [ ] 60 FPS stable
- [ ] Playable start to finish (30-60 min)

---

## Next Steps After Vertical Slice

**If slice is fun:**
1. Get feedback from 5-10 playtesters
2. Iterate on combat feel
3. Expand to full game (Floors 4-10, Elevations 3-5)

**If slice needs work:**
1. Identify what's not fun
2. Prototype fixes
3. Re-test

**If slice fails:**
1. Pivot to different combat system
2. Or shelve project and start fresh

---

**Estimated Timeline:** 3-6 months (solo dev, part-time)

**Critical Path:**
- Week 1-2: Attack refactor
- Week 3-4: Core systems (Progression, Artifacts)
- Week 5-6: Spells
- Week 7-8: Enemies + combat
- Week 9-12: Levels + boss
- Week 13-15: UI + save/load
- Week 16-18: Polish + test

**End Goal:** Playable 30-60 min slice that proves the game is fun.
