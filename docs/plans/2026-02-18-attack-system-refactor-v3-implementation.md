# Attack System Refactor v3 — Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Replace Resource-as-orchestrator pattern with Godot-native node composition: Skill scenes (spawn logic), Projectile scripts (lifecycle), Resources (pure data).

**Architecture:** Skills are persistent child nodes of AttackManager. SpawnComponents inside Skills handle instantiation patterns (single, burst). Projectiles are Area2D with a base script that wires hitbox, movement (injected via factory), and lifetime. AttackData returns to pure data.

**Tech Stack:** Godot 4.6, C# (.NET 10.0), no tests — verify with `dotnet build` + F5

**Design doc:** `docs/plans/2026-02-18-attack-system-refactor-v3.md`

---

### Task 1: Strip AttackData back to pure data

Remove all behavior from AttackData and ProjectileData. They become data-only Resources.

**Files:**
- Modify: `Core/Combat/Attacks/AttackData.cs`
- Modify: `Core/Combat/Attacks/ProjectileData.cs`
- Delete: `Core/Combat/Attacks/BurstAttackData.cs`
- Delete: `Core/Combat/Attacks/BurstAttackData.cs.uid`

**Step 1: Simplify AttackData.cs**

Replace full file with:

```csharp
using Godot;

namespace RotOfTime.Core.Combat.Attacks;

[GlobalClass]
public partial class AttackData : Resource
{
    [Export] public string Name { get; set; } = "Unnamed Attack";
    [Export] public float DamageCoefficient { get; set; } = 1.0f;
    [Export] public float CooldownDuration { get; set; }
}
```

Removed: `AttackScene`, `Spawn()`, `FindHitbox()`, `using` for Components namespace.

**Step 2: Simplify ProjectileData.cs**

Replace full file with:

```csharp
using Godot;

namespace RotOfTime.Core.Combat.Attacks;

[GlobalClass]
public partial class ProjectileData : AttackData
{
    [Export] public int InitialSpeed { get; set; } = 200;
    [Export] public int TargetSpeed { get; set; } = 200;
    [Export] public double Acceleration { get; set; }
    [Export] public int Lifetime { get; set; } = 5;
    [Export] public MovementType MovementType { get; set; } = MovementType.Linear;
}
```

Removed: `Spawn()`, `OnLifetimeExpired()`, `FindMovementComponent()`, `using` for AttackMovementComponents.

**Step 3: Delete BurstAttackData**

Delete `Core/Combat/Attacks/BurstAttackData.cs` and its `.uid` file.

**Step 4: Build**

Run: `dotnet build`
Expected: Build FAILS — `AttackManagerComponent.SpawnAttack()` still calls `data.Spawn(ctx)`, `IceShard.tres` references `BurstAttackData`. This is expected; we fix consumers in subsequent tasks.

**Step 5: Commit**

```
feat: strip AttackData/ProjectileData to pure data, delete BurstAttackData
```

---

### Task 2: Create MovementType enum and MovementFactory

**Files:**
- Create: `Core/Combat/Attacks/MovementType.cs`
- Create: `Core/Combat/Attacks/MovementFactory.cs`

**Step 1: Create MovementType.cs**

```csharp
namespace RotOfTime.Core.Combat.Attacks;

public enum MovementType
{
    Linear
}
```

**Step 2: Create MovementFactory.cs**

```csharp
using RotOfTime.Core.Combat.Components.AttackMovementComponents;

namespace RotOfTime.Core.Combat.Attacks;

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

**Step 3: Commit**

```
feat: add MovementType enum and MovementFactory
```

---

### Task 3: Create Projectile.cs base script

The Projectile gets its own script. It exports HitboxComponent, and in `Initialize()` it wires hitbox, finds injected movement component, sets up lifetime timer, and connects impact signal.

**Files:**
- Create: `Scenes/Attacks/Projectiles/Projectile.cs`

**Step 1: Create Projectile.cs**

```csharp
using Godot;
using RotOfTime.Core.Combat.Attacks;
using RotOfTime.Core.Combat.Components;
using RotOfTime.Core.Combat.Components.AttackMovementComponents;

namespace RotOfTime.Scenes.Attacks.Projectiles;

public partial class Projectile : Area2D
{
    [Export] public AttackHitboxComponent HitboxComponent { get; set; }

    public void Initialize(AttackContext ctx, AttackData data)
    {
        // Wire hitbox
        HitboxComponent?.Initialize(ctx.OwnerStats, data, ctx.DamageMultiplier);

        // Wire movement (injected as child by SpawnComponent before Initialize)
        if (data is ProjectileData projData)
        {
            var movement = FindMovementComponent();
            movement?.Initialize(projData, ctx.Direction);

            // Lifetime timer
            var timer = new Timer
            {
                WaitTime = projData.Lifetime,
                OneShot = true
            };
            timer.Timeout += OnLifetimeExpired;
            AddChild(timer);
            timer.Start();
        }

        // Destroy on impact
        if (HitboxComponent != null)
            HitboxComponent.AttackConnected += OnImpact;
    }

    protected virtual void OnImpact()
    {
        QueueFree();
    }

    protected virtual void OnLifetimeExpired()
    {
        QueueFree();
    }

    private AttackMovementComponent FindMovementComponent()
    {
        foreach (var child in GetChildren())
        {
            if (child is AttackMovementComponent movement)
                return movement;
        }
        return null;
    }
}
```

**Step 2: Commit**

```
feat: add Projectile.cs base script with Initialize/OnImpact/OnLifetimeExpired
```

---

### Task 4: Create Fireball.cs (extends Projectile)

For now it's just a stub that inherits Projectile. The OnImpact override for explosion will come later when we have explosion effects.

**Files:**
- Create: `Scenes/Attacks/Projectiles/Fireball/Fireball.cs`

**Step 1: Create Fireball.cs**

```csharp
namespace RotOfTime.Scenes.Attacks.Projectiles;

public partial class Fireball : Projectile
{
    // Override OnImpact() here when explosion effect is ready
}
```

**Step 2: Commit**

```
feat: add Fireball.cs stub extending Projectile
```

---

### Task 5: Create AttackSpawnComponent hierarchy

Three files: abstract base, SingleSpawnComponent, BurstSpawnComponent.

**Files:**
- Create: `Core/Combat/Components/AttackSpawnComponents/AttackSpawnComponent.cs`
- Create: `Core/Combat/Components/AttackSpawnComponents/SingleSpawnComponent.cs`
- Create: `Core/Combat/Components/AttackSpawnComponents/BurstSpawnComponent.cs`

**Step 1: Create AttackSpawnComponent.cs**

```csharp
using Godot;
using RotOfTime.Core.Combat.Attacks;

namespace RotOfTime.Core.Combat.Components.AttackSpawnComponents;

public abstract partial class AttackSpawnComponent : Node
{
    [Signal]
    public delegate void SkillFiredEventHandler(float cooldown);

    [Export] public PackedScene ProjectileScene { get; set; }
    [Export] public AttackData Data { get; set; }

    public abstract void Execute(AttackContext ctx);
}
```

**Step 2: Create SingleSpawnComponent.cs**

```csharp
using Godot;
using RotOfTime.Core.Combat.Attacks;
using RotOfTime.Scenes.Attacks.Projectiles;

namespace RotOfTime.Core.Combat.Components.AttackSpawnComponents;

[GlobalClass]
public partial class SingleSpawnComponent : AttackSpawnComponent
{
    public override void Execute(AttackContext ctx)
    {
        if (ProjectileScene == null || Data == null)
        {
            GD.PrintErr("SingleSpawnComponent: ProjectileScene or Data is null.");
            return;
        }

        SpawnProjectile(ctx);
        EmitSignal(SignalName.SkillFired, Data.CooldownDuration);
    }

    protected void SpawnProjectile(AttackContext ctx)
    {
        var instance = ProjectileScene.Instantiate<Node2D>();
        instance.GlobalPosition = ctx.SpawnPosition;
        instance.Rotation = ctx.Direction.Angle();

        // Inject movement component if projectile data
        if (Data is ProjectileData projData)
        {
            var movement = MovementFactory.Create(projData.MovementType);
            instance.AddChild(movement);
        }

        ctx.AttacksContainer.AddChild(instance);

        // Initialize if it's a Projectile
        if (instance is Projectile projectile)
            projectile.Initialize(ctx, Data);
    }
}
```

**Step 3: Create BurstSpawnComponent.cs**

```csharp
using Godot;
using RotOfTime.Core.Combat.Attacks;
using RotOfTime.Scenes.Attacks.Projectiles;

namespace RotOfTime.Core.Combat.Components.AttackSpawnComponents;

[GlobalClass]
public partial class BurstSpawnComponent : AttackSpawnComponent
{
    [Export] public int BurstCount { get; set; } = 3;
    [Export] public float BurstDelay { get; set; } = 0.1f;

    private AttackContext _currentCtx;
    private int _fired;
    private Timer _burstTimer;

    public override void Execute(AttackContext ctx)
    {
        if (ProjectileScene == null || Data == null)
        {
            GD.PrintErr("BurstSpawnComponent: ProjectileScene or Data is null.");
            return;
        }

        _currentCtx = ctx;
        _fired = 0;

        // Fire first immediately
        SpawnOne();
        EmitSignal(SignalName.SkillFired, Data.CooldownDuration);

        if (BurstCount <= 1)
            return;

        // Timer for remaining
        _burstTimer = new Timer
        {
            WaitTime = BurstDelay,
            OneShot = false
        };
        _burstTimer.Timeout += OnBurstTick;
        AddChild(_burstTimer);
        _burstTimer.Start();
    }

    private void OnBurstTick()
    {
        SpawnOne();

        if (_fired >= BurstCount)
        {
            _burstTimer.Stop();
            _burstTimer.QueueFree();
            _burstTimer = null;
        }
    }

    private void SpawnOne()
    {
        // Use owner's current position for each sub-projectile
        var ctx = _currentCtx with { SpawnPosition = _currentCtx.Owner.GlobalPosition };

        var instance = ProjectileScene.Instantiate<Node2D>();
        instance.GlobalPosition = ctx.SpawnPosition;
        instance.Rotation = ctx.Direction.Angle();

        if (Data is ProjectileData projData)
        {
            var movement = MovementFactory.Create(projData.MovementType);
            instance.AddChild(movement);
        }

        ctx.AttacksContainer.AddChild(instance);

        if (instance is Projectile projectile)
            projectile.Initialize(ctx, Data);

        _fired++;
    }
}
```

**Step 4: Commit**

```
feat: add AttackSpawnComponent hierarchy (Single, Burst)
```

---

### Task 6: Refactor AttackManagerComponent to use Skills

The manager changes from `data.Spawn(ctx)` to skill-based: stores PackedScene per slot, instantiates skills as children, connects `SkillFired` signal for cooldown.

**Files:**
- Modify: `Core/Combat/Components/AttackManagerComponent.cs`

**Step 1: Rewrite AttackManagerComponent.cs**

```csharp
using System;
using System.Collections.Generic;
using Godot;
using RotOfTime.Core.Combat.Attacks;
using RotOfTime.Core.Combat.Components.AttackSpawnComponents;
using RotOfTime.Core.Entities;

namespace RotOfTime.Core.Combat.Components;

public abstract partial class AttackManagerComponent<TSlot> : Node
    where TSlot : struct, Enum
{
    private readonly Dictionary<TSlot, AttackSpawnComponent> _slotSkills = new();
    private readonly Dictionary<TSlot, Timer> _slotTimers = new();

    [Signal]
    public delegate void AttackFiredEventHandler(StringName slot);

    protected void RegisterSkill(TSlot slotKey, PackedScene skillScene)
    {
        if (skillScene == null)
        {
            GD.PrintErr($"AttackManagerComponent: Skill scene is null for slot '{slotKey}'.");
            return;
        }

        var skillInstance = skillScene.Instantiate<Node2D>();
        AddChild(skillInstance);

        AttackSpawnComponent spawnComponent = null;
        foreach (var child in skillInstance.GetChildren())
        {
            if (child is AttackSpawnComponent spawn)
            {
                spawnComponent = spawn;
                break;
            }
        }

        if (spawnComponent == null)
        {
            GD.PrintErr($"AttackManagerComponent: No AttackSpawnComponent found in skill for slot '{slotKey}'.");
            skillInstance.QueueFree();
            return;
        }

        _slotSkills[slotKey] = spawnComponent;

        // Create cooldown timer
        var timer = new Timer
        {
            OneShot = true,
            Name = $"{slotKey}Timer"
        };
        AddChild(timer);
        _slotTimers[slotKey] = timer;

        // Connect signal for cooldown
        spawnComponent.SkillFired += (cooldown) => OnSkillFired(slotKey, cooldown);
    }

    private void OnSkillFired(TSlot slotKey, float cooldown)
    {
        if (_slotTimers.TryGetValue(slotKey, out var timer))
            timer.Start(cooldown);

        EmitSignal(SignalName.AttackFired, SlotToStringName(slotKey));
    }

    public bool TryFire(TSlot slotKey, Vector2 direction, Vector2 position, EntityStats stats, Node2D ownerNode)
    {
        if (!_slotSkills.TryGetValue(slotKey, out var spawnComponent))
            return false;

        if (!_slotTimers.TryGetValue(slotKey, out var timer))
            return false;

        if (!timer.IsStopped())
            return false;

        var attackContainer = GetTree().Root.GetNodeOrNull<Node>("Main/Attacks");
        if (attackContainer == null)
        {
            GD.PrintErr("AttackManagerComponent: 'Main/Attacks' container not found.");
            return false;
        }

        var ctx = new AttackContext(direction, position, stats, ownerNode, 1.0f, attackContainer);
        spawnComponent.Execute(ctx);
        return true;
    }

    public bool IsOnCooldown(TSlot slotKey)
    {
        if (!_slotTimers.TryGetValue(slotKey, out var timer))
            return false;
        return !timer.IsStopped();
    }

    public float GetCooldownProgress(TSlot slotKey)
    {
        if (!_slotTimers.TryGetValue(slotKey, out var timer))
            return 0f;
        if (timer.IsStopped())
            return 0f;
        return Mathf.Clamp((float)(timer.TimeLeft / timer.WaitTime), 0f, 1f);
    }

    public bool HasSlot(TSlot slotKey)
    {
        return _slotSkills.ContainsKey(slotKey);
    }

    private static StringName SlotToStringName(TSlot slot)
    {
        return slot.ToString();
    }

    public static bool TryParseSlot(StringName name, out TSlot slot)
    {
        return Enum.TryParse(name.ToString(), out slot);
    }
}
```

Removed: `_slotData` dict, `RegisterSlot()`, `RegisterTimer()`, `UnregisterSlot()`, `SpawnAttack()`, `GetSlotData()`.
Added: `_slotSkills` dict, `RegisterSkill()`, `OnSkillFired()`. Timers are created inside `RegisterSkill()` now.

**Step 2: Commit**

```
refactor: AttackManagerComponent uses Skill scenes instead of data.Spawn()
```

---

### Task 7: Refactor PlayerAttackManager and EnemyAttackManager

Change exports from AttackData to PackedScene and use RegisterSkill().

**Files:**
- Modify: `Scenes/Player/Components/PlayerAttackManager.cs`
- Modify: `Scenes/Enemies/BasicEnemy/Components/EnemyAttackManager.cs`

**Step 1: Rewrite PlayerAttackManager.cs**

```csharp
using Godot;
using RotOfTime.Core.Combat.Components;

namespace RotOfTime.Scenes.Player.Components;

[GlobalClass]
public partial class PlayerAttackManager : AttackManagerComponent<PlayerAttackSlot>
{
    [Export] public PackedScene BasicAttackSkill { get; set; }
    [Export] public PackedScene Spell1Skill { get; set; }
    [Export] public PackedScene Spell2Skill { get; set; }

    public override void _Ready()
    {
        if (BasicAttackSkill != null)
            RegisterSkill(PlayerAttackSlot.BasicAttack, BasicAttackSkill);

        if (Spell1Skill != null)
            RegisterSkill(PlayerAttackSlot.Spell1, Spell1Skill);

        if (Spell2Skill != null)
            RegisterSkill(PlayerAttackSlot.Spell2, Spell2Skill);
    }
}
```

**Step 2: Rewrite EnemyAttackManager.cs**

```csharp
using Godot;
using RotOfTime.Core.Combat.Components;

namespace RotOfTime.Scenes.Enemies.BasicEnemy.Components;

[GlobalClass]
public partial class EnemyAttackManager : AttackManagerComponent<EnemyAttackSlot>
{
    [Export] public PackedScene BodyAttackSkill { get; set; }

    public override void _Ready()
    {
        if (BodyAttackSkill != null)
            RegisterSkill(EnemyAttackSlot.BodyAttack, BodyAttackSkill);
    }
}
```

**Step 3: Commit**

```
refactor: PlayerAttackManager and EnemyAttackManager use PackedScene skills
```

---

### Task 8: Update AttackContext record

Minor cleanup — remove the doc comment referencing `AttackData.Spawn()`.

**Files:**
- Modify: `Core/Combat/Attacks/AttackContext.cs`

**Step 1: Update AttackContext.cs**

```csharp
using Godot;
using RotOfTime.Core.Entities;

namespace RotOfTime.Core.Combat.Attacks;

/// <summary>
///     Packages all world context needed to execute an attack skill.
///     Built by AttackManagerComponent, consumed by AttackSpawnComponent and Projectile.
/// </summary>
public record AttackContext(
    Vector2 Direction,
    Vector2 SpawnPosition,
    EntityStats OwnerStats,
    Node2D Owner,
    float DamageMultiplier,
    Node AttacksContainer
);
```

**Step 2: Commit**

```
docs: update AttackContext doc comment
```

---

### Task 9: Update scene files (.tscn)

Remove LinearMovementComponent from Projectile.tscn and FireBall.tscn. Add Projectile.cs script to Projectile.tscn. Add Fireball.cs script to FireBall.tscn. Wire HitboxComponent export.

**Files:**
- Modify: `Scenes/Attacks/Projectiles/Projectile.tscn`
- Modify: `Scenes/Attacks/Projectiles/Fireball/FireBall.tscn`

**Step 1: Rewrite Projectile.tscn**

```
[gd_scene load_steps=4 format=3 uid="uid://dwips18xqhtp8"]

[ext_resource type="Texture2D" uid="uid://dknoyppxxm06p" path="res://Scenes/Attacks/Body/RockBody/rock_body.png" id="2_t7gc1"]
[ext_resource type="Script" path="res://Scenes/Attacks/Projectiles/Projectile.cs" id="3_proj"]
[ext_resource type="Script" uid="uid://dmjy7sqfwj6nb" path="res://Core/Combat/Components/AttackHitboxComponent.cs" id="6_utkwb"]

[sub_resource type="CircleShape2D" id="CircleShape2D_pyv0a"]
radius = 7.071068

[sub_resource type="CircleShape2D" id="CircleShape2D_mbvye"]
radius = 8.0

[node name="Projectile" type="Area2D" node_paths=PackedStringArray("HitboxComponent")]
collision_layer = 512
collision_mask = 0
script = ExtResource("3_proj")
HitboxComponent = NodePath("AttackHitboxComponent")

[node name="Sprite2D" type="Sprite2D" parent="."]
position = Vector2(-9.536743e-07, -9.536743e-07)
scale = Vector2(0.40625, 0.40625)
texture = ExtResource("2_t7gc1")

[node name="CollisionShape2D" type="CollisionShape2D" parent="."]
shape = SubResource("CircleShape2D_pyv0a")

[node name="AttackHitboxComponent" type="Area2D" parent="."]
script = ExtResource("6_utkwb")
metadata/_custom_type_script = "uid://dmjy7sqfwj6nb"

[node name="CollisionShape2D2" type="CollisionShape2D" parent="AttackHitboxComponent"]
shape = SubResource("CircleShape2D_mbvye")
debug_color = Color(0.9788559, 0, 0.3966187, 0.41960785)
```

Removed: LinearMovementComponent node and its ext_resource.
Added: Projectile.cs script on root, `node_paths` + `HitboxComponent` NodePath export.

**Step 2: Rewrite FireBall.tscn**

```
[gd_scene load_steps=4 format=3 uid="uid://do7qyryvmcnir"]

[ext_resource type="Texture2D" uid="uid://dknoyppxxm06p" path="res://Scenes/Attacks/Body/RockBody/rock_body.png" id="3_sprite"]
[ext_resource type="Script" uid="uid://dmjy7sqfwj6nb" path="res://Core/Combat/Components/AttackHitboxComponent.cs" id="4_nvk5s"]
[ext_resource type="Script" path="res://Scenes/Attacks/Projectiles/Fireball/Fireball.cs" id="5_fb"]

[sub_resource type="CircleShape2D" id="CircleShape2D_6oscf"]
radius = 7.071068

[sub_resource type="CircleShape2D" id="CircleShape2D_nvk5s"]
radius = 8.0

[node name="Fireball" type="Area2D" node_paths=PackedStringArray("HitboxComponent")]
collision_layer = 512
collision_mask = 0
script = ExtResource("5_fb")
HitboxComponent = NodePath("AttackHitboxComponent")

[node name="Sprite2D" type="Sprite2D" parent="."]
scale = Vector2(0.5, 0.5)
texture = ExtResource("3_sprite")

[node name="CollisionShape2D" type="CollisionShape2D" parent="."]
shape = SubResource("CircleShape2D_6oscf")

[node name="AttackHitboxComponent" type="Area2D" parent="."]
script = ExtResource("4_nvk5s")
metadata/_custom_type_script = "uid://dmjy7sqfwj6nb"

[node name="CollisionShape2D" type="CollisionShape2D" parent="AttackHitboxComponent"]
shape = SubResource("CircleShape2D_nvk5s")
```

Removed: LinearMovementComponent node and its ext_resource.
Added: Fireball.cs script on root, `node_paths` + `HitboxComponent` NodePath export.

**Step 3: Commit**

```
refactor: update Projectile.tscn and FireBall.tscn — remove movement, add scripts
```

---

### Task 10: Create Skill scenes (.tscn)

Three skill scenes + one for the enemy body attack.

**Files:**
- Create: `Scenes/Attacks/Skills/CarbonBoltSkill.tscn`
- Create: `Scenes/Attacks/Skills/FireballSkill.tscn`
- Create: `Scenes/Attacks/Skills/IceShardSkill.tscn`
- Create: `Scenes/Attacks/Skills/RockBodySkill.tscn`

**Step 1: Create CarbonBoltSkill.tscn**

```
[gd_scene format=3]

[ext_resource type="Script" path="res://Core/Combat/Components/AttackSpawnComponents/SingleSpawnComponent.cs" id="1_spawn"]
[ext_resource type="PackedScene" uid="uid://dwips18xqhtp8" path="res://Scenes/Attacks/Projectiles/Projectile.tscn" id="2_proj"]
[ext_resource type="Resource" uid="uid://dbbdpgopbl6w5" path="res://Resources/Attacks/CarbonBolt.tres" id="3_data"]

[node name="CarbonBoltSkill" type="Node2D"]

[node name="SingleSpawnComponent" type="Node" parent="."]
script = ExtResource("1_spawn")
ProjectileScene = ExtResource("2_proj")
Data = ExtResource("3_data")
```

**Step 2: Create FireballSkill.tscn**

```
[gd_scene format=3]

[ext_resource type="Script" path="res://Core/Combat/Components/AttackSpawnComponents/SingleSpawnComponent.cs" id="1_spawn"]
[ext_resource type="PackedScene" uid="uid://do7qyryvmcnir" path="res://Scenes/Attacks/Projectiles/Fireball/FireBall.tscn" id="2_proj"]
[ext_resource type="Resource" path="res://Resources/Attacks/Fireball.tres" id="3_data"]

[node name="FireballSkill" type="Node2D"]

[node name="SingleSpawnComponent" type="Node" parent="."]
script = ExtResource("1_spawn")
ProjectileScene = ExtResource("2_proj")
Data = ExtResource("3_data")
```

**Step 3: Create IceShardSkill.tscn**

```
[gd_scene format=3]

[ext_resource type="Script" path="res://Core/Combat/Components/AttackSpawnComponents/BurstSpawnComponent.cs" id="1_spawn"]
[ext_resource type="PackedScene" uid="uid://dwips18xqhtp8" path="res://Scenes/Attacks/Projectiles/Projectile.tscn" id="2_proj"]
[ext_resource type="Resource" uid="uid://023j08qnbkdp" path="res://Resources/Attacks/IceShardProjectile.tres" id="3_data"]

[node name="IceShardSkill" type="Node2D"]

[node name="BurstSpawnComponent" type="Node" parent="."]
script = ExtResource("1_spawn")
ProjectileScene = ExtResource("2_proj")
Data = ExtResource("3_data")
BurstCount = 3
BurstDelay = 0.1
```

**Step 4: Create RockBodySkill.tscn**

The enemy melee attack. Uses SingleSpawnComponent but with RockBody.tscn (Area2D without Projectile script). The SpawnComponent handles it gracefully — it checks `instance is Projectile` before calling Initialize.

```
[gd_scene format=3]

[ext_resource type="Script" path="res://Core/Combat/Components/AttackSpawnComponents/SingleSpawnComponent.cs" id="1_spawn"]
[ext_resource type="PackedScene" uid="uid://rockbody" path="res://Scenes/Attacks/Body/RockBody/RockBody.tscn" id="2_scene"]
[ext_resource type="Resource" uid="uid://g5hkmvpkqu8s" path="res://Scenes/Attacks/Body/RockBody/RockBodyAttackData.tres" id="3_data"]

[node name="RockBodySkill" type="Node2D"]

[node name="SingleSpawnComponent" type="Node" parent="."]
script = ExtResource("1_spawn")
ProjectileScene = ExtResource("2_scene")
Data = ExtResource("3_data")
```

**Nota:** RockBody.tscn no tiene Projectile.cs — es un Area2D puro con hitbox. SingleSpawnComponent lo instancia, le inyecta el movimiento (no pasará nada porque Data es AttackData, no ProjectileData), lo añade al tree. Pero hay un problema: el hitbox no se inicializa porque `instance is Projectile` es false.

Necesitamos que SingleSpawnComponent también inicialice el hitbox directamente para scenes sin script Projectile. Ajustamos SingleSpawnComponent.SpawnProjectile():

En el `SpawnProjectile` method, después de `ctx.AttacksContainer.AddChild(instance)`, cambiar el bloque final:

```csharp
// Initialize — Projectile script handles its own init, otherwise init hitbox directly
if (instance is Projectile projectile)
{
    projectile.Initialize(ctx, Data);
}
else
{
    // Direct hitbox init for scenes without Projectile script (e.g., RockBody)
    foreach (var child in instance.GetChildren())
    {
        if (child is AttackHitboxComponent hitbox)
        {
            hitbox.Initialize(ctx.OwnerStats, Data, ctx.DamageMultiplier);
            break;
        }
    }
}
```

Esto requiere añadir `using RotOfTime.Core.Combat.Components;` al SingleSpawnComponent.

Mismo patrón para BurstSpawnComponent.SpawnOne() (aunque en la práctica bursts siempre usan Projectile).

**Step 5: Commit**

```
feat: create Skill scenes (CarbonBolt, Fireball, IceShard, RockBody)
```

---

### Task 11: Update .tres resource files

Remove `AttackScene` field from all resources. Add `MovementType` to ProjectileData resources.

**Files:**
- Modify: `Resources/Attacks/CarbonBolt.tres`
- Modify: `Resources/Attacks/Fireball.tres`
- Modify: `Resources/Attacks/IceShardProjectile.tres`
- Modify: `Resources/Attacks/IceShard.tres` → DELETE (replaced by IceShardSkill.tscn)
- Modify: `Scenes/Attacks/Body/RockBody/RockBodyAttackData.tres`

**Step 1: Update CarbonBolt.tres**

```
[gd_resource type="Resource" script_class="ProjectileData" format=3 uid="uid://dbbdpgopbl6w5"]

[ext_resource type="Script" uid="uid://bi7dj6vs1ad3f" path="res://Core/Combat/Attacks/ProjectileData.cs" id="2_n1ove"]

[resource]
script = ExtResource("2_n1ove")
InitialSpeed = 300
TargetSpeed = 300
Lifetime = 3
MovementType = 0
Name = "Carbon bolt"
DamageCoefficient = 1.0
CooldownDuration = 0.4
metadata/_custom_type_script = "uid://bi7dj6vs1ad3f"
```

Removed: AttackScene ext_resource and field. Added: `MovementType = 0` (Linear).

**Step 2: Update Fireball.tres**

```
[gd_resource type="Resource" script_class="ProjectileData" format=3]

[ext_resource type="Script" uid="uid://bi7dj6vs1ad3f" path="res://Core/Combat/Attacks/ProjectileData.cs" id="2_script"]

[resource]
script = ExtResource("2_script")
InitialSpeed = 200
TargetSpeed = 200
Lifetime = 4
MovementType = 0
Name = "Fireball"
DamageCoefficient = 1.5
CooldownDuration = 2.0
metadata/_custom_type_script = "uid://bi7dj6vs1ad3f"
```

**Step 3: Update IceShardProjectile.tres**

```
[gd_resource type="Resource" script_class="ProjectileData" format=3 uid="uid://023j08qnbkdp"]

[ext_resource type="Script" uid="uid://bi7dj6vs1ad3f" path="res://Core/Combat/Attacks/ProjectileData.cs" id="1_script"]

[resource]
script = ExtResource("1_script")
InitialSpeed = 350
TargetSpeed = 350
Lifetime = 3
MovementType = 0
Name = "Ice Shard Projectile"
DamageCoefficient = 0.5
metadata/_custom_type_script = "uid://bi7dj6vs1ad3f"
```

**Step 4: Delete IceShard.tres**

`Resources/Attacks/IceShard.tres` — no longer needed. Burst config lives in IceShardSkill.tscn's BurstSpawnComponent.

**Step 5: Update RockBodyAttackData.tres**

```
[gd_resource type="Resource" script_class="AttackData" format=3 uid="uid://g5hkmvpkqu8s"]

[ext_resource type="Script" uid="uid://boqlkvifp1inw" path="res://Core/Combat/Attacks/AttackData.cs" id="1_p3pox"]

[resource]
script = ExtResource("1_p3pox")
Name = "RockBody"
DamageCoefficient = 1.0
CooldownDuration = 1.5
```

Removed: AttackScene ext_resource and field.

**Step 6: Commit**

```
refactor: update .tres resources — remove AttackScene, add MovementType
```

---

### Task 12: Update Player.tscn and BasicEnemy.tscn exports

The PlayerAttackManager and EnemyAttackManager now export PackedScene instead of AttackData. Update the .tscn files to wire the new skill scenes.

**Files:**
- Modify: `Scenes/Player/Player.tscn` — change PlayerAttackManager exports
- Modify: `Scenes/Enemies/BasicEnemy/BasicEnemy.tscn` — change EnemyAttackManager exports

**Step 1: Update Player.tscn**

Find the PlayerAttackManager node and replace AttackData references with Skill PackedScene references. The exact ext_resource IDs and node structure depend on the current .tscn content. Read the file, find the PlayerAttackManager section, and:

- Remove ext_resources for CarbonBolt.tres, Fireball.tres, IceShard.tres
- Add ext_resources for CarbonBoltSkill.tscn, FireballSkill.tscn, IceShardSkill.tscn
- Change `BasicAttackData`, `Spell1Data`, `Spell2Data` properties to `BasicAttackSkill`, `Spell1Skill`, `Spell2Skill`

**Step 2: Update BasicEnemy.tscn**

Find the EnemyAttackManager node and:

- Remove ext_resource for RockBodyAttackData.tres
- Add ext_resource for RockBodySkill.tscn
- Change `BodyAttackData` property to `BodyAttackSkill`

**Step 3: Build and verify**

Run: `dotnet build`
Expected: BUILD SUCCESS — all references resolved, no missing types.

**Step 4: Commit**

```
refactor: wire Skill scenes into Player.tscn and BasicEnemy.tscn
```

---

### Task 13: Cleanup — delete dead code and update CLAUDE.md

**Files:**
- Verify no remaining references to deleted types
- Update: `CLAUDE.md` — Architecture section, Attack System docs, Decisions Log

**Step 1: Search for dead references**

Grep for: `BurstAttackData`, `IAttack`, `data.Spawn`, `AttackScene`, `RegisterSlot`, `RegisterTimer`, `GetSlotData`, `UnregisterSlot`

Any remaining references need updating.

**Step 2: Update CLAUDE.md**

Update the Attack System section in Architecture to reflect:
- Skills as persistent children of AttackManager
- SpawnComponents (Single, Burst)
- Projectile.cs base class
- MovementFactory
- Remove IAttack references
- Update data flow diagram

Add Decisions Log entry for v3 refactor.

**Step 3: Final build**

Run: `dotnet build`
Expected: BUILD SUCCESS

**Step 4: Manual test in Godot (F5)**

Test:
- LMB → Carbon Bolt fires, moves straight, destroys on hit or timeout
- Key 1 → Fireball fires, moves straight, destroys on hit or timeout
- Key 2 → Ice Shard fires 3 projectiles in burst
- Enemy → RockBody melee attack works when player is in range
- All cooldowns work

**Step 5: Commit**

```
chore: cleanup dead references, update CLAUDE.md for v3 architecture
```

---

## Task dependency order

```
Task 1 (strip data) ─┐
Task 2 (factory)     ─┼─→ Task 5 (spawn components) ─→ Task 6 (manager) ─→ Task 7 (player/enemy managers)
Task 3 (Projectile)  ─┤                                                       │
Task 4 (Fireball)    ─┘                                                       ▼
                                                            Task 8 (context) ─→ Task 9 (scenes .tscn)
                                                                                  │
                                                                                  ▼
                                                            Task 10 (skill .tscn) ─→ Task 11 (.tres)
                                                                                       │
                                                                                       ▼
                                                                     Task 12 (Player/Enemy .tscn) ─→ Task 13 (cleanup)
```

Tasks 1-4 can be done in parallel. Task 5 depends on 1-4. Then sequential from 6 onward.

## Build checkpoints

- After Task 5: `dotnet build` will FAIL (AttackManagerComponent still uses old API)
- After Task 7: `dotnet build` should PASS (all C# code updated)
- After Task 12: `dotnet build` PASS + scenes wired correctly
- After Task 13: Full manual test in Godot (F5)
