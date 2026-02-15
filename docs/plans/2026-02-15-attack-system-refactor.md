# Attack System Refactoring Plan

**Date:** 2026-02-15  
**Goal:** Simplify attack system from overengineered 5-layer abstraction to straightforward spell system  
**Timeline:** 1-2 weeks  

---

## Current Architecture (Overcomplicated)

```
Player.cs
  └─ PlayerAttackManager (generic manager with TSlot enum)
      ├─ BasicAttack (AttackSlot node)
      │   ├─ AttackData (Resource: damage, cooldown, scene)
      │   └─ SpawnerComponent (SingleShot/Burst/Turret node)
      ├─ Attack1 (AttackSlot node)
      └─ Attack2 (AttackSlot node)
```

**Problems:**
- AttackSlot manages cooldown/cast state manually (reinventing Timer)
- SpawnerComponent abstraction adds layer for 3 simple patterns
- 4 attack slots when we only need 3 (Basic + 2 Spells)
- Too many files to modify to add a new spell

---

## New Architecture (Simplified)

```
Player.cs
  └─ PlayerAttackManager
      ├─ BasicAttackData (AttackData resource)
      ├─ Spell1Data (AttackData resource)
      ├─ Spell2Data (AttackData resource)
      ├─ BasicAttackTimer (Timer node)
      ├─ Spell1Timer (Timer node)
      └─ Spell2Timer (Timer node)
```

**Spell scenes are self-contained:**
- Each spell implements IAttack
- Spell handles its own movement/spawning logic
- No separate spawner component layer

---

## Step-by-Step Refactor

### Phase 1: Simplify PlayerAttackSlot Enum

**File:** `Scenes/Player/PlayerAttackSlot.cs`

**Current (4 slots):**
```csharp
public enum PlayerAttackSlot {
    BasicAttack,
    Ability1,
    Ability2,
    Ability3
}
```

**New (3 slots):**
```csharp
public enum PlayerAttackSlot {
    BasicAttack,
    Spell1,
    Spell2
}
```

---

### Phase 2: Simplify AttackManagerComponent

**File:** `Core/Combat/Components/AttackManagerComponent.cs`

**Changes:**
1. Remove manual cooldown tracking (`_cooldownRemaining -= dt`)
2. Use Timer nodes instead
3. Remove cast timing logic (no casting in simplified design)
4. Remove spawner activation logic

**New Structure:**
```csharp
public abstract partial class AttackManagerComponent<TSlot> : Node
{
    private Dictionary<TSlot, AttackData> _slotData = new();
    private Dictionary<TSlot, Timer> _slotTimers = new();
    
    public bool TryFire(TSlot slot, Vector2 direction, Vector2 position, 
        EntityStats stats, Node2D owner)
    {
        if (!_slotData.TryGetValue(slot, out var data))
            return false;
            
        if (!_slotTimers[slot].IsStopped())
            return false; // On cooldown
        
        SpawnAttack(data, direction, position, stats);
        _slotTimers[slot].Start(data.CooldownDuration);
        return true;
    }
    
    private void SpawnAttack(AttackData data, Vector2 dir, Vector2 pos, EntityStats stats)
    {
        var attack = data.AttackScene.Instantiate<Node2D>();
        attack.GlobalPosition = pos;
        attack.Rotation = dir.Angle();
        
        var container = GetTree().Root.GetNode<Node2D>("Main/Attacks");
        container.AddChild(attack);
        
        if (attack is IAttack iAttack)
            iAttack.Execute(dir, stats, data);
    }
}
```

---

### Phase 3: Simplify PlayerAttackManager

**File:** `Scenes/Player/Components/PlayerAttackManager.cs`

**Current:**
```csharp
public AttackSlot BasicAttackSlot { get; set; }
public AttackSlot Ability1Slot { get; set; }
public AttackSlot Ability2Slot { get; set; }
public AttackSlot Ability3Slot { get; set; }
```

**New:**
```csharp
[Export] public AttackData BasicAttackData { get; set; }
[Export] public AttackData Spell1Data { get; set; }
[Export] public AttackData Spell2Data { get; set; }

public override void _Ready()
{
    RegisterSlot(PlayerAttackSlot.BasicAttack, BasicAttackData);
    RegisterSlot(PlayerAttackSlot.Spell1, Spell1Data);
    RegisterSlot(PlayerAttackSlot.Spell2, Spell2Data);
    
    CreateTimerForSlot(PlayerAttackSlot.BasicAttack);
    CreateTimerForSlot(PlayerAttackSlot.Spell1);
    CreateTimerForSlot(PlayerAttackSlot.Spell2);
}

private void CreateTimerForSlot(PlayerAttackSlot slot)
{
    var timer = new Timer();
    timer.OneShot = true;
    AddChild(timer);
    RegisterTimer(slot, timer);
}
```

---

### Phase 4: Update Player.cs Input Mapping

**File:** `Scenes/Player/Player.cs`

**Current:**
```csharp
private PlayerAttackSlot? GetPressedAttackSlot()
{
    if (EntityInputComponent.IsAttackJustPressed)
        return PlayerAttackSlot.BasicAttack;
    if (EntityInputComponent.IsAbility1JustPressed)
        return PlayerAttackSlot.Ability1;
    if (EntityInputComponent.IsAbility2JustPressed)
        return PlayerAttackSlot.Ability2;
    if (EntityInputComponent.IsAbility3JustPressed)
        return PlayerAttackSlot.Ability3;
    return null;
}
```

**New:**
```csharp
private PlayerAttackSlot? GetPressedAttackSlot()
{
    if (EntityInputComponent.IsAttackJustPressed)
        return PlayerAttackSlot.BasicAttack;
    if (EntityInputComponent.IsSpell1JustPressed)
        return PlayerAttackSlot.Spell1;
    if (EntityInputComponent.IsSpell2JustPressed)
        return PlayerAttackSlot.Spell2;
    return null;
}
```

---

### Phase 5: Simplify AttackData Resource

**File:** `Core/Combat/Attacks/AttackData.cs`

**Remove cast-related fields:**
```csharp
[GlobalClass]
public partial class AttackData : Resource
{
    [Export] public string Name { get; set; } = "Unnamed Attack";
    [Export] public float DamageCoefficient { get; set; } = 1.0f;
    [Export] public PackedScene AttackScene { get; set; }
    [Export] public float CooldownDuration { get; set; } = 1.0f;
    
    // REMOVED: CastDuration, AllowMovementDuringCast, IsInstantCast
}
```

**Reasoning:** No casting phase in simplified design. All attacks are instant.

---

### Phase 6: Remove AttackSlot.cs

**File:** `Core/Combat/Components/AttackSlot.cs`

**Action:** DELETE this file entirely.

**Reasoning:** 
- Cooldown now handled by Timer nodes
- No cast timing needed
- No spawner coordination needed
- All functionality moved to AttackManagerComponent

---

### Phase 7: Remove SpawnerComponent Abstraction

**Files to DELETE:**
- `Core/Combat/Components/AttackSpawnerComponents/AttackSpawnerComponent.cs`
- `Core/Combat/Components/AttackSpawnerComponents/SingleShotSpawner.cs`
- `Core/Combat/Components/AttackSpawnerComponents/BurstSpawner.cs`
- `Core/Combat/Components/AttackSpawnerComponents/TurretSpawner.cs`

**Migration Strategy:**

**Instead of:**
```
AttackSlot → SingleShotSpawner → Fireball scene
```

**Now:**
```
AttackManagerComponent → Fireball scene (handles own spawning)
```

**Example: Burst Attack (self-contained)**
```csharp
// Scenes/Attacks/Projectiles/BurstFireball.cs
public partial class BurstFireball : Node2D, IAttack
{
    private int _burstCount = 3;
    private float _burstDelay = 0.25f;
    
    public async void Execute(Vector2 direction, EntityStats stats, AttackData data)
    {
        for (int i = 0; i < _burstCount; i++)
        {
            SpawnProjectile(direction, stats, data);
            
            if (i < _burstCount - 1)
                await ToSignal(GetTree().CreateTimer(_burstDelay), "timeout");
        }
        
        QueueFree(); // Clean up spawner after burst complete
    }
    
    private void SpawnProjectile(Vector2 dir, EntityStats stats, AttackData data)
    {
        var projectile = /* instantiate Fireball scene */;
        // Set position, rotation, execute
    }
}
```

---

## Phase 8: Update Player.tscn Scene

**Current Structure:**
```
PlayerAttackManager
  ├─ BasicAttack (AttackSlot)
  │   └─ SingleShotSpawner
  ├─ Attack1 (AttackSlot)
  │   └─ BurstSpawner
  ├─ Attack2 (AttackSlot)
  └─ Attack3 (AttackSlot)
```

**New Structure:**
```
PlayerAttackManager
  (No children - just exports AttackData resources)
```

**Exported Properties in Inspector:**
- BasicAttackData → Fireball.tres
- Spell1Data → IceShard.tres
- Spell2Data → (empty for now)

---

## Phase 9: Remove Casting State

**File:** `Scenes/Player/StateMachine/States/CastingState.cs`

**Action:** DELETE if no longer needed (depends on if you want cast animations)

**Alternative:** Keep for animation purposes only, but simplify:
```csharp
public override void Update(double delta)
{
    // Just play animation, no complex timing
    if (AnimationFinished())
        TransitionTo("IdleState");
}
```

---

## Phase 10: Update EntityInputComponent

**File:** `Core/Entities/Components/EntityInputComponent.cs`

**Add Spell Inputs:**
```csharp
public bool IsAttackJustPressed => Input.IsActionJustPressed("attack");
public bool IsSpell1JustPressed => Input.IsActionJustPressed("spell_1");
public bool IsSpell2JustPressed => Input.IsActionJustPressed("spell_2");

// REMOVE:
// IsAbility1JustPressed, IsAbility2JustPressed, IsAbility3JustPressed
```

**Update Input Map:**
- Add `spell_1` → Key: 1
- Add `spell_2` → Key: 2
- Remove `ability_3`

---

## Testing Checklist

After refactor, verify:

- [ ] Basic attack fires on click
- [ ] Spell 1 fires on key 1
- [ ] Spell 2 fires on key 2
- [ ] Cooldowns work (can't spam)
- [ ] Attacks spawn at correct position
- [ ] Attacks aim toward mouse cursor
- [ ] Damage calculation still works (Hitbox → Hurtbox)
- [ ] Player can move while attacking
- [ ] No errors in console

---

## Files Modified Summary

**Modified:**
- `Scenes/Player/PlayerAttackSlot.cs` (4→3 slots)
- `Core/Combat/Components/AttackManagerComponent.cs` (use Timers)
- `Scenes/Player/Components/PlayerAttackManager.cs` (export AttackData)
- `Scenes/Player/Player.cs` (update input mapping)
- `Core/Combat/Attacks/AttackData.cs` (remove cast fields)
- `Core/Entities/Components/EntityInputComponent.cs` (spell inputs)
- `Scenes/Player/Player.tscn` (remove AttackSlot children)

**Deleted:**
- `Core/Combat/Components/AttackSlot.cs`
- `Core/Combat/Components/AttackSpawnerComponents/*` (entire folder)
- `Scenes/Player/StateMachine/States/CastingState.cs` (optional)

**New:**
- Example spell scenes with self-contained spawn logic

---

## Benefits of Refactor

**Before:**
- 5 layers of abstraction
- ~600 lines of attack infrastructure code
- Hard to add new spells (create Data + Slot + Spawner + Scene)

**After:**
- 2 layers (Manager → Spell Scene)
- ~200 lines of attack infrastructure code
- Easy to add spells (create Scene + AttackData, done)

**Development Speed:**
- Adding new spell: 30 minutes → 10 minutes
- Debugging attack issues: Find across 5 files → Find in 1 file
- Balancing cooldowns: Edit in Godot Inspector (Timer)

---

## Migration Strategy (Safe Approach)

1. **Create new branch:** `git checkout -b refactor/attack-system-simplification`
2. **Refactor in phases** (commit after each phase)
3. **Test after each commit**
4. **Keep old code in separate branch** for reference
5. **Merge only when all tests pass**

---

**Next Steps:**
1. Review this plan
2. Decide: Full refactor now OR defer until after vertical slice?
3. If proceeding: Start with Phase 1 (enum simplification)

**Recommendation:** Do this refactor BEFORE vertical slice so you're building on clean foundation.
