# Attack System Optimization Design

## Context

The attack system works correctly but has accumulated unnecessary complexity. This design documents seven targeted optimizations to simplify the code without changing gameplay behavior.

Current flow: `Input → Player.TryFireAttack() → AttackManager.TryFire() → AttackSlot.Activate() → Spawner.SpawnAttackInstance() → IAttack.Execute() → Hitbox/Hurtbox collision → EntityStatsComponent.TakeDamage()`.

The system involves ~27 files across `Core/Combat/`, `Scenes/Attacks/`, and `Scenes/Player/`. Each optimization below targets a specific friction point.

---

## Optimization 1: Merge AttackDamageComponent into AttackHitboxComponent

### Problem

Every attack requires two tightly coupled components:
- `AttackDamageComponent` — calculates damage, configures faction collision layers
- `AttackHitboxComponent` — Area2D that detects collisions

`AttackDamageComponent` holds an `[Export]` reference to `AttackHitboxComponent` and writes directly to its properties (`CollisionLayer`, `CollisionMask`, `AttackResult`). They are inseparable in practice.

### Change

Merge all `AttackDamageComponent` logic into `AttackHitboxComponent`. The unified component becomes an Area2D that both detects collisions and manages damage calculation.

```csharp
[GlobalClass]
public partial class AttackHitboxComponent : Area2D
{
    private static readonly Dictionary<GameConstants.Faction, GameConstants.GameLayers> AttackLayerMap = new()
    {
        { GameConstants.Faction.Player, GameConstants.GameLayers.PlayerAttack },
        { GameConstants.Faction.Ally, GameConstants.GameLayers.PlayerAttack },
        { GameConstants.Faction.Enemy, GameConstants.GameLayers.EnemyAttack }
    };

    private static readonly Dictionary<GameConstants.Faction, GameConstants.GameLayers> TargetMaskMap = new()
    {
        { GameConstants.Faction.Player, GameConstants.GameLayers.EnemyDamageBox },
        { GameConstants.Faction.Ally, GameConstants.GameLayers.EnemyDamageBox },
        { GameConstants.Faction.Enemy, GameConstants.GameLayers.PlayerDamageBox }
    };

    [Signal]
    public delegate void AttackConnectedEventHandler();

    [Export] public GameConstants.Faction Faction { get; set; }

    public AttackResult AttackResult { get; private set; } = AttackResult.None;

    public void Initialize(EntityStats ownerStats, AttackData attackData, float damageMultiplier = 1.0f)
    {
        ApplyFaction(ownerStats.Faction);
        AttackResult = CalculateDamage(ownerStats, attackData, damageMultiplier);
    }

    private AttackResult CalculateDamage(EntityStats ownerStats, AttackData attackData, float damageMultiplier)
    {
        var baseResult = DamageCalculator.CalculateRawDamage(ownerStats, attackData);
        if (Mathf.IsEqualApprox(damageMultiplier, 1.0f))
            return baseResult;
        int modified = Mathf.Max(1, (int)(baseResult.RawDamage * damageMultiplier));
        return new AttackResult(modified, baseResult.AttackName, baseResult.IsCritical);
    }

    private void ApplyFaction(GameConstants.Faction faction)
    {
        Faction = faction;
        CollisionLayer = (uint)AttackLayerMap[faction];
        CollisionMask = (uint)TargetMaskMap[faction];
    }

    public override void _Ready()
    {
        AreaEntered += OnAreaEntered;
    }

    private void OnAreaEntered(Area2D area)
    {
        if (area is not HurtboxComponent) return;
        EmitSignal(SignalName.AttackConnected);
    }

    public override void _ExitTree()
    {
        AreaEntered -= OnAreaEntered;
    }
}
```

### Files affected

- **Delete:** `Core/Combat/Components/AttackDamageComponent.cs`
- **Modify:** `Core/Combat/Components/AttackHitboxComponent.cs` (absorb merged logic)
- **Modify:** `Scenes/Attacks/Projectiles/Projectile.cs` (remove DamageComponent export, use HitboxComponent directly)
- **Modify:** `Scenes/Attacks/Body/RockBody/RockBody.cs` (same)
- **Modify:** All `.tscn` files that had AttackDamageComponent nodes (remove them, wire hitbox directly)

---

## Optimization 2: AddChild Before Execute

### Problem

In `AttackSpawnerComponent.SpawnAttackInstance()`, the current order is:

```csharp
var node = attackData.AttackScene.Instantiate<Node2D>();
attack.Execute(direction, position, ownerStats, attackData, damageMultiplier); // Node NOT in tree
GetTree().CurrentScene.AddChild(node);                                         // _Ready() runs here
```

`Execute()` runs before `_Ready()`. In `Projectile`, this means `_projectileData` is saved in `Execute()` and consumed in `_Ready()`. This works but creates a fragile two-phase initialization where both methods must coordinate through instance fields.

### Change

Reverse the order: `AddChild` first, then `Execute`.

```csharp
protected void SpawnAttackInstance(Vector2 direction, Vector2 position, EntityStats ownerStats, AttackData attackData)
{
    if (attackData?.AttackScene == null)
    {
        GD.PrintErr($"AttackSpawnerComponent: AttackScene not assigned in AttackData on {Name}");
        return;
    }

    var node = attackData.AttackScene.Instantiate<Node2D>();

    // Position the node before adding to tree
    if (node is Node2D node2D)
    {
        node2D.GlobalPosition = position;
        node2D.Rotation = direction.Angle();
    }

    // Add to attack container in Main scene
    var container = GetTree().Root.GetNode("Main/Attacks");
    container.AddChild(node);  // _Ready() runs here

    // Now Execute with node fully in tree
    if (node is IAttack attack)
    {
        attack.Execute(direction, ownerStats, attackData, DamageMultiplier);
    }
    else
    {
        GD.PrintErr($"AttackSpawnerComponent: Scene does not implement IAttack");
        node.QueueFree();
    }
}
```

This lets `Execute()` assume the node is fully initialized (components resolved, `_Ready()` called). `Projectile._Ready()` can set up timers and signals unconditionally, and `Execute()` handles all data injection in one pass.

### Files affected

- **Modify:** `Core/Combat/Components/AttackSpawnerComponents/AttackSpawnerComponent.cs`
- **Modify:** `Scenes/Attacks/Projectiles/Projectile.cs` (consolidate initialization into Execute)
- **Modify:** `Scenes/Attacks/Body/RockBody/RockBody.cs` (same)
- **Add:** Node `Attacks` (Node2D) to `Scenes/Main/Main.tscn`

---

## Optimization 3: Simplify IAttack.Execute Signature

### Problem

`IAttack.Execute()` receives 5 parameters including `position`, but with Optimization 2 the spawner already positions the node before calling Execute.

### Change

Remove `position` from the interface. The spawner handles positioning; Execute handles behavior initialization.

```csharp
public interface IAttack
{
    void Execute(Vector2 direction, EntityStats ownerStats, AttackData attackData,
                 float damageMultiplier = 1.0f);
}
```

### Projectile.Execute after both optimizations

```csharp
public void Execute(Vector2 direction, EntityStats ownerStats, AttackData attackData,
                    float damageMultiplier = 1.0f)
{
    Direction = direction.Normalized();

    // Initialize movement from ProjectileData (node is in tree, MovementComponent is ready)
    if (attackData is ProjectileData projectileData)
    {
        MovementComponent.Initialize(projectileData);
        _lifetimeTimer.WaitTime = projectileData.Lifetime;
        _lifetimeTimer.Start();
    }

    // Initialize damage on the unified hitbox
    HitboxComponent.Initialize(ownerStats, attackData, damageMultiplier);
}
```

### Files affected

- **Modify:** `Core/Combat/Attacks/IAttack.cs`
- **Modify:** `Scenes/Attacks/Projectiles/Projectile.cs`
- **Modify:** `Scenes/Attacks/Body/RockBody/RockBody.cs`

---

## Optimization 4: Immutable AttackResult

### Problem

`AttackResult` extends `Resource` with mutable `set` properties. Since it passes through signals and is assigned to hitboxes, immutability prevents accidental modification.

### Change

Replace `set` with `init` on all properties.

```csharp
[GlobalClass]
public partial class AttackResult : Resource
{
    public AttackResult() { }

    public AttackResult(int rawDamage, string attackName, bool isCritical = false)
    {
        RawDamage = rawDamage;
        AttackName = attackName;
        IsCritical = isCritical;
    }

    [Export] public int RawDamage { get; init; }
    [Export] public string AttackName { get; init; } = "";
    [Export] public bool IsCritical { get; init; }

    public static AttackResult None => new(0, "None");
}
```

### Files affected

- **Modify:** `Core/Combat/Results/AttackResult.cs`

---

## Optimization 5: Attack Container Node in Main

### Problem

`AttackSpawnerComponent` adds attack instances directly to `GetTree().CurrentScene`, cluttering the scene tree and making batch cleanup impossible.

### Change

Add a permanent `Attacks` (Node2D) node to `Scenes/Main/Main.tscn`. The spawner references it by path: `GetTree().Root.GetNode("Main/Attacks")`.

Benefits:
- Clean scene tree (all attacks grouped)
- Batch cleanup via `Attacks.GetChildren()` + QueueFree
- Visible in editor for debugging

### Files affected

- **Modify:** `Scenes/Main/Main.tscn` (add Attacks Node2D)
- **Modify:** `Core/Combat/Components/AttackSpawnerComponents/AttackSpawnerComponent.cs` (change AddChild target)

---

## Optimization 6: BurstSpawner Follows Owner Position

### Problem

`BurstSpawner` captures `_position` and `_direction` on the first shot and reuses them for all subsequent shots. In an ARPG with constant movement, shots 2+ spawn at the player's stale position.

### Change

`BurstSpawner` receives a reference to the owner node and reads its current position each shot. The `AttackSpawnerComponent.Activate()` signature gains an `ownerNode` parameter.

```csharp
public abstract void Activate(Vector2 direction, Vector2 position, EntityStats ownerStats,
                               AttackData attackData, Node2D ownerNode);
```

`BurstSpawner` stores the owner reference and reads fresh position/direction each spawn:

```csharp
public override void Process(double delta)
{
    if (IsComplete) return;
    _timeSinceLastSpawn += (float)delta;

    while (_timeSinceLastSpawn >= DelayBetweenShots && _spawned < Count)
    {
        Vector2 currentPos = _ownerNode.GlobalPosition + _direction * 16;
        SpawnAttackInstance(_direction, currentPos, _ownerStats, _attackData);
        _spawned++;
        _timeSinceLastSpawn -= DelayBetweenShots;
    }
}
```

Note: Direction stays fixed from activation. Only position updates. If direction should also follow, that's a separate feature (aim-tracked burst) not covered here.

### Files affected

- **Modify:** `Core/Combat/Components/AttackSpawnerComponents/AttackSpawnerComponent.cs` (add ownerNode to Activate)
- **Modify:** `Core/Combat/Components/AttackSpawnerComponents/BurstSpawner.cs` (store and use ownerNode)
- **Modify:** `Core/Combat/Components/AttackSpawnerComponents/SingleShotSpawner.cs` (accept param, ignore it)
- **Modify:** `Core/Combat/Components/AttackSlot.cs` (pass ownerNode through)
- **Modify:** `Core/Combat/Components/AttackManagerComponent.cs` (pass ownerNode through TryFire)
- **Modify:** `Scenes/Player/Player.cs` (pass self as ownerNode)

---

## Optimization 7: New TurretSpawner for Fixed-Position Bursts

### Problem

The old `BurstSpawner` behavior (fixed position/direction) is still valid for certain attacks — turret-style abilities, traps, or placed effects that fire from where they were cast.

### Change

Create `TurretSpawner` that preserves the old `BurstSpawner` behavior:

```csharp
[GlobalClass]
public partial class TurretSpawner : AttackSpawnerComponent
{
    [Export] public int Count { get; set; } = 3;
    [Export] public float DelayBetweenShots { get; set; } = 0.25f;

    private Vector2 _fixedDirection;
    private Vector2 _fixedPosition;
    private EntityStats _ownerStats;
    private AttackData _attackData;
    private int _spawned;
    private float _timeSinceLastSpawn;

    public override bool IsComplete => _spawned >= Count;

    public override void Activate(Vector2 direction, Vector2 position, EntityStats ownerStats,
                                   AttackData attackData, Node2D ownerNode)
    {
        _fixedDirection = direction.Normalized();
        _fixedPosition = position;
        _ownerStats = ownerStats;
        _attackData = attackData;
        // ownerNode intentionally ignored — turret fires from fixed position

        SpawnAttackInstance(_fixedDirection, _fixedPosition, _ownerStats, _attackData);
        _spawned = 1;
        _timeSinceLastSpawn = 0f;
    }

    public override void Process(double delta)
    {
        if (IsComplete) return;
        _timeSinceLastSpawn += (float)delta;

        while (_timeSinceLastSpawn >= DelayBetweenShots && _spawned < Count)
        {
            SpawnAttackInstance(_fixedDirection, _fixedPosition, _ownerStats, _attackData);
            _spawned++;
            _timeSinceLastSpawn -= DelayBetweenShots;
        }
    }

    public override void Reset()
    {
        _spawned = 0;
        _timeSinceLastSpawn = 0f;
    }
}
```

### Files affected

- **Add:** `Core/Combat/Components/AttackSpawnerComponents/TurretSpawner.cs`

---

## Optimized Attack Flow (After All Changes)

```
1. Input → EntityInputComponent.GetPressedAttackSlot()
2. Player.TryFireAttack() → direction, spawn position
3. PlayerAttackManager.TryFire(slot, dir, pos, stats, ownerNode)
4. AttackSlot.Activate() → starts cooldown, resets spawner, activates spawner
5. Spawner.SpawnAttackInstance():
   a. Instantiate scene
   b. Position node (GlobalPosition, Rotation)
   c. AddChild to Main/Attacks container → _Ready() runs
   d. IAttack.Execute(direction, stats, data, multiplier) → initializes behavior
6. AttackHitboxComponent.Initialize() → calculates AttackResult, sets layers
7. [Physics] Hitbox overlaps Hurtbox
8. HurtboxComponent reads hitbox.AttackResult → emits AttackReceived
9. EntityStatsComponent.TakeDamage() → DamageCalculator → applies damage
```

### Classes removed

- `AttackDamageComponent` (merged into `AttackHitboxComponent`)

### Classes added

- `TurretSpawner` (fixed-position multi-shot pattern)

### Net complexity change

- 1 fewer class, cleaner initialization order, immutable data, organized scene tree

---

## Implementation Order

1. AttackResult immutability (Opt 4) — smallest change, no dependencies
2. Merge AttackDamageComponent into AttackHitboxComponent (Opt 1) — foundational
3. Add Attacks node to Main.tscn (Opt 5) — needed for step 4
4. AddChild before Execute + simplify IAttack signature (Opt 2 + 3) — depends on 1, 2, 5
5. BurstSpawner follows owner (Opt 6) — independent of 1-4
6. TurretSpawner (Opt 7) — new file, depends on 6 for consistent API
