# Stats System Redesign — Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Reemplazar el sistema de stats basado en multiplicadores flotantes por un sistema discreto de máscaras (1 máscara = 10 HP), eliminando DefenseStat, corrigiendo el bug de DamageMultiplier nunca aplicado, y añadiendo enforcement interno en AttackResult y EntityStats.

**Architecture:** El enforcement vive dentro de los propios Resources — `EntityStats.VitalityStat` setter clampea a múltiplo de 10, `AttackResult` constructor clampea a múltiplo de 5. `HealthMultiplier` (float) se reemplaza por `MaxHealthBonus` (int). `DamageResult` se elimina. El bug del `AttackManagerComponent` hardcodeando `damageMultiplier = 1.0f` se corrige propagando el valor desde `PlayerStatsComponent`.

**Tech Stack:** Godot 4.6, C# (.NET 10.0). Verificación: `dotnet build` desde raíz del proyecto.

---

## Grupos de cambios

Los cambios están agrupados por acoplamiento. Cada tarea deja el build en verde antes del commit.

---

### Task 1: Enforcement en AttackResult

El constructor de `AttackResult` clampea `RawDamage` a múltiplo de 5 con threshold 3.
Regla: 0–2 → 0, 3–4 → 5, 5–7 → 5, 8–9 → 10, etc.

**Files:**
- Modify: `Core/Combat/Results/AttackResult.cs`

**Step 1: Modificar el constructor de AttackResult**

```csharp
// Core/Combat/Results/AttackResult.cs
using Godot;

namespace RotOfTime.Core.Combat.Results;

public partial class AttackResult : Resource
{
    public AttackResult() { }

    public AttackResult(int rawDamage, string attackName, bool isCritical = false)
    {
        RawDamage  = ClampToMaskUnit(rawDamage);
        AttackName = attackName;
        IsCritical = isCritical;
    }

    public int    RawDamage  { get; init; }
    public string AttackName { get; init; } = "";
    public bool   IsCritical { get; init; }

    public static AttackResult None => new(0, "None");

    /// <summary>
    /// Clampea a múltiplo de 5. Threshold 3: 0-2→0, 3-4→5, 5-7→5, 8-9→10, etc.
    /// Garantiza que todo daño sea media máscara o máscara completa.
    /// </summary>
    private static int ClampToMaskUnit(int raw)
    {
        if (raw <= 0) return 0;
        int lower = (raw / 5) * 5;
        return (raw % 5) >= 3 ? lower + 5 : lower;
    }
}
```

**Step 2: Verificar build**

```bash
dotnet build
```
Esperado: Build succeeded, 0 errores.

**Step 3: Commit**

```bash
git add Core/Combat/Results/AttackResult.cs
git commit -m "feat: enforcement de daño a múltiplos de 5 en AttackResult"
```

---

### Task 2: Enforcement en EntityStats + eliminar DefenseStat

`VitalityStat` setter clampea a múltiplo de 10. Se elimina `DefenseStat`.
Este cambio romperá `DamageCalculator` — se corrige en la misma tarea.

**Files:**
- Modify: `Core/Entities/EntityStats.cs`
- Modify: `Core/Combat/Calculations/DamageCalculator.cs`
- Delete: `Core/Combat/Results/DamageResult.cs`
- Modify: `Core/Entities/Components/EntityStatsComponent.cs`

**Step 1: Reescribir EntityStats**

```csharp
// Core/Entities/EntityStats.cs
using Godot;

namespace RotOfTime.Core.Entities;

[GlobalClass]
public partial class EntityStats : Resource
{
    public EntityStats(int vitalityStat, int attackStat,
        GameConstants.Faction faction = GameConstants.Faction.Player)
    {
        VitalityStat = vitalityStat;   // setter aplica clamp
        AttackStat   = attackStat;
        Faction      = faction;
    }

    public EntityStats() : this(0, 0) { }

    private int _vitalityStat;

    /// <summary>
    /// Vida base. Siempre múltiplo de 10 (1 máscara = 10 HP).
    /// El setter clampea automáticamente: 45→40, 55→50.
    /// </summary>
    [Export]
    public int VitalityStat
    {
        get => _vitalityStat;
        set => _vitalityStat = (value / 10) * 10;
    }

    [Export] public int AttackStat { get; set; }
    [Export] public GameConstants.Faction Faction { get; set; } = GameConstants.Faction.Player;
}
```

**Step 2: Actualizar DamageCalculator — eliminar CalculateFinalDamage, añadir damageMultiplier**

```csharp
// Core/Combat/Calculations/DamageCalculator.cs
using Godot;
using RotOfTime.Core.Combat.Attacks;
using RotOfTime.Core.Combat.Results;
using RotOfTime.Core.Entities;

namespace RotOfTime.Core.Combat.Calculations;

/// <summary>
/// Cálculo de daño centralizado. Sin defensa — la defensa es esquivar.
/// </summary>
public static class DamageCalculator
{
    /// <summary>
    /// Calcula el daño final del ataque aplicando el multiplicador.
    /// El AttackResult resultante ya tiene el daño clampado a múltiplo de 5.
    /// </summary>
    public static AttackResult CalculateAttack(EntityStats attacker, AttackData data, float damageMultiplier = 1.0f)
    {
        int raw = Mathf.RoundToInt(attacker.AttackStat * data.DamageCoefficient * damageMultiplier);
        return new AttackResult(raw, data.Name);
    }
}
```

**Step 3: Eliminar DamageResult.cs**

Borrar el archivo `Core/Combat/Results/DamageResult.cs`. No tiene más referencias tras eliminar `CalculateFinalDamage`.

**Step 4: Actualizar EntityStatsComponent**

```csharp
// Core/Entities/Components/EntityStatsComponent.cs
using System;
using Godot;
using RotOfTime.Core.Combat.Results;

namespace RotOfTime.Core.Entities.Components;

[GlobalClass]
public partial class EntityStatsComponent : Node
{
    [Signal] public delegate void EntityDiedEventHandler();
    [Signal] public delegate void HealthChangedEventHandler(int newHealth);
    [Signal] public delegate void StatsUpdatedEventHandler();

    [Export] public EntityStats EntityStats;

    public int CurrentHealth { get; private set; }

    /// <summary>
    /// Bonus plano de HP (en unidades HP, múltiplos de 10).
    /// Sumado a EntityStats.VitalityStat para obtener MaxHealth.
    /// Fuentes: progresión (elevaciones) + artefactos.
    /// </summary>
    public int MaxHealthBonus { get; set; } = 0;

    /// <summary>Multiplicador de daño. Aplicado al AttackStat al disparar.</summary>
    public float DamageMultiplier { get; set; } = 1.0f;

    /// <summary>HP máximo = vida base + bonus de elevaciones y artefactos.</summary>
    public int MaxHealth => EntityStats.VitalityStat + MaxHealthBonus;

    /// <summary>Attack power efectivo con multiplicador aplicado.</summary>
    public int AttackPower => Mathf.RoundToInt(EntityStats.AttackStat * DamageMultiplier);

    public override void _Ready()
    {
        if (EntityStats == null)
            throw new InvalidOperationException("EntityStats is not set");

        CurrentHealth = MaxHealth;
    }

    public void TakeDamage(AttackResult attackResult)
    {
        // Sin defensa — el daño del AttackResult ya es el daño final.
        CurrentHealth = Math.Max(0, CurrentHealth - attackResult.RawDamage);
        EmitSignal(SignalName.HealthChanged, CurrentHealth);

        if (CurrentHealth <= 0)
            EmitSignal(SignalName.EntityDied);
    }

    /// <summary>
    /// Recalcula MaxHealth y clampea CurrentHealth. Llamar tras cambiar MaxHealthBonus.
    /// </summary>
    public void RecalculateStats()
    {
        if (CurrentHealth > MaxHealth)
            CurrentHealth = MaxHealth;

        EmitSignal(SignalName.StatsUpdated);
    }

    public void ResetStats()
    {
        CurrentHealth = MaxHealth;
    }
}
```

**Step 5: Verificar build**

```bash
dotnet build
```
Esperado: Build succeeded, 0 errores.
Si hay errores de `DamageResult` o `DefenseStat` en otros archivos, buscarlos con:
```bash
grep -r "DamageResult\|DefenseStat\|CalculateFinalDamage\|HealthMultiplier" --include="*.cs" .
```
Y corregir cada referencia.

**Step 6: Commit**

```bash
git add Core/Entities/EntityStats.cs \
        Core/Combat/Calculations/DamageCalculator.cs \
        Core/Entities/Components/EntityStatsComponent.cs
git rm Core/Combat/Results/DamageResult.cs
git commit -m "refactor: eliminar DefenseStat y DamageResult, enforcement VitalityStat, MaxHealthBonus"
```

---

### Task 3: Artifacts — HealthBonus float → int

`ArtifactData.HealthBonus` cambia de `float` a `int`. `ArtifactManager.GetTotalHealthBonus()` retorna `int`.

**Files:**
- Modify: `Core/Artifacts/ArtifactData.cs`
- Modify: `Core/Artifacts/ArtifactManager.cs`

**Step 1: Actualizar ArtifactData**

Cambiar la propiedad `HealthBonus`:
```csharp
// Antes:
[Export] public float HealthBonus { get; set; }

// Después:
/// <summary>
/// Bonus de HP en unidades HP (múltiplos de 10). 10 = +1 máscara, 20 = +2 máscaras.
/// </summary>
[Export] public int HealthBonus { get; set; }
```

**Step 2: Actualizar ArtifactManager.GetTotalHealthBonus**

```csharp
// Antes:
public float GetTotalHealthBonus() => _equipped.Sum(t => LoadData(t).HealthBonus);

// Después:
public int GetTotalHealthBonus() => _equipped.Sum(t => LoadData(t).HealthBonus);
```

**Step 3: Verificar build**

```bash
dotnet build
```
Esperado: Build succeeded. Si `PlayerStatsComponent` falla por tipo, se corrige en Task 5.

**Step 4: Commit**

```bash
git add Core/Artifacts/ArtifactData.cs Core/Artifacts/ArtifactManager.cs
git commit -m "refactor: ArtifactData.HealthBonus de float a int (unidades HP)"
```

---

### Task 4: ProgressionManager — GetHealthBonus

Eliminar `GetHealthMultiplier()`. Añadir `GetHealthBonus()` que retorna bonus plano por elevación.

**Files:**
- Modify: `Core/Progression/ProgressionManager.cs`

**Step 1: Actualizar ProgressionManager**

```csharp
// Eliminar este método:
public float GetHealthMultiplier() => 1.0f + ActivatedResonances * 0.20f;

// Añadir este método:
/// <summary>
/// Bonus plano de HP por elevaciones completadas. +10 HP por elevación = +1 máscara.
/// Elevation 1 (inicio): +0. Elevation 2 cleared: +10. ... Elevation 5 cleared: +40.
/// </summary>
public int GetHealthBonus() => (CurrentElevation - 1) * 10;
```

`GetDamageMultiplier()` no cambia.

**Step 2: Verificar build**

```bash
dotnet build
```
Esperado: Build succeeded. Si `PlayerStatsComponent` falla, es esperado — se corrige en Task 5.

**Step 3: Commit**

```bash
git add Core/Progression/ProgressionManager.cs
git commit -m "refactor: GetHealthMultiplier → GetHealthBonus (int plano por elevación)"
```

---

### Task 5: PlayerStatsComponent — conectar nuevas APIs

Actualizar `RecalculateFromManagers` para usar `MaxHealthBonus` (int) en lugar de `HealthMultiplier` (float).

**Files:**
- Modify: `Scenes/Player/Components/PlayerStatsComponent.cs`

**Step 1: Actualizar PlayerStatsComponent**

```csharp
// Scenes/Player/Components/PlayerStatsComponent.cs
using Godot;
using RotOfTime.Autoload;
using RotOfTime.Core.Entities.Components;

namespace RotOfTime.Scenes.Player.Components;

/// <summary>
/// Extiende EntityStatsComponent con recálculo automático desde
/// ProgressionManager y ArtifactManager.
/// </summary>
[GlobalClass]
public partial class PlayerStatsComponent : EntityStatsComponent
{
    public override void _Ready()
    {
        base._Ready();
        SubscribeToManagerEvents();
        RecalculateFromManagers();
        ResetStats();
    }

    public override void _ExitTree()
    {
        UnsubscribeFromManagerEvents();
    }

    private void SubscribeToManagerEvents()
    {
        var prog = GameManager.Instance?.ProgressionManager;
        var arts = GameManager.Instance?.ArtifactManager;

        if (prog != null) prog.StatsChanged += RecalculateFromManagers;
        if (arts != null) arts.StatsChanged += RecalculateFromManagers;
    }

    private void UnsubscribeFromManagerEvents()
    {
        var prog = GameManager.Instance?.ProgressionManager;
        var arts = GameManager.Instance?.ArtifactManager;

        if (prog != null) prog.StatsChanged -= RecalculateFromManagers;
        if (arts != null) arts.StatsChanged -= RecalculateFromManagers;
    }

    private void RecalculateFromManagers()
    {
        var prog = GameManager.Instance?.ProgressionManager;
        var arts = GameManager.Instance?.ArtifactManager;

        // HP: bonus plano (int). Elevaciones + artefactos. Sin multiplicadores flotantes.
        MaxHealthBonus = (prog?.GetHealthBonus() ?? 0) + (arts?.GetTotalHealthBonus() ?? 0);

        // Daño: multiplicador aditivo. Resonancias + artefactos.
        DamageMultiplier = (prog?.GetDamageMultiplier() ?? 1.0f) + (arts?.GetTotalDamageBonus() ?? 0f);

        RecalculateStats();
    }
}
```

**Step 2: Verificar build**

```bash
dotnet build
```
Esperado: Build succeeded, 0 errores.

**Step 3: Commit**

```bash
git add Scenes/Player/Components/PlayerStatsComponent.cs
git commit -m "refactor: PlayerStatsComponent usa MaxHealthBonus (int) y GetHealthBonus"
```

---

### Task 6: Bug fix — DamageMultiplier en AttackManagerComponent

El bug real: `AttackManagerComponent.TryFire` hardcodea `1.0f` como `damageMultiplier` en el `AttackContext`. Se añade el parámetro al método `TryFire`.

**Files:**
- Modify: `Core/Combat/Components/AttackManagerComponent.cs`
- Modify: `Scenes/Player/Player.cs`

**Step 1: Añadir parámetro damageMultiplier a TryFire**

```csharp
// Core/Combat/Components/AttackManagerComponent.cs
// Cambiar la firma del método TryFire:

// Antes:
public bool TryFire(TSlot slotKey, Vector2 direction, Vector2 position, EntityStats stats, Node2D ownerNode)
{
    // ...
    var ctx = new AttackContext(direction, position, stats, ownerNode, 1.0f, attackContainer);

// Después:
public bool TryFire(TSlot slotKey, Vector2 direction, Vector2 position,
                    EntityStats stats, Node2D ownerNode, float damageMultiplier = 1.0f)
{
    // ...
    var ctx = new AttackContext(direction, position, stats, ownerNode, damageMultiplier, attackContainer);
```

**Step 2: Pasar DamageMultiplier desde Player.TryFireAttack**

```csharp
// Scenes/Player/Player.cs — método TryFireAttack
// Antes:
bool fired = AttackManager.TryFire(
    slot.Value, dir, spawnPos, EntityStatsComponent.EntityStats, this);

// Después:
bool fired = AttackManager.TryFire(
    slot.Value, dir, spawnPos, EntityStatsComponent.EntityStats, this,
    EntityStatsComponent.DamageMultiplier);
```

**Step 3: Verificar build**

```bash
dotnet build
```
Esperado: Build succeeded, 0 errores.

**Step 4: Commit**

```bash
git add Core/Combat/Components/AttackManagerComponent.cs Scenes/Player/Player.cs
git commit -m "fix: propagar DamageMultiplier al AttackContext (daño de resonancias no se aplicaba)"
```

---

### Task 7: AttackHitboxComponent — usar DamageCalculator actualizado

`AttackHitboxComponent.CalculateDamage` llama al viejo `DamageCalculator.CalculateRawDamage`. Actualizar para usar `DamageCalculator.CalculateAttack`.

**Files:**
- Modify: `Core/Combat/Components/AttackHitboxComponent.cs`

**Step 1: Actualizar CalculateDamage en AttackHitboxComponent**

```csharp
// Core/Combat/Components/AttackHitboxComponent.cs
// Reemplazar el método CalculateDamage completo:

private AttackResult CalculateDamage(EntityStats ownerStats, AttackData attackData, float damageMultiplier)
{
    if (attackData == null)
        return AttackResult.None;

    return DamageCalculator.CalculateAttack(ownerStats, attackData, damageMultiplier);
}
```

Eliminar el import de `using RotOfTime.Core.Combat.Results;` si ya no se usa directamente (lo usa `AttackResult`, que sigue siendo del mismo namespace — verificar que los usings queden correctos).

**Step 2: Verificar build**

```bash
dotnet build
```
Esperado: Build succeeded, 0 errores.

**Step 3: Commit**

```bash
git add Core/Combat/Components/AttackHitboxComponent.cs
git commit -m "refactor: AttackHitboxComponent usa DamageCalculator.CalculateAttack"
```

---

### Task 8: Actualizar valores en .tres

Actualizar los recursos de artefactos y el EnemyBolt para reflejar la nueva escala.

**Files:**
- Modify: `Core/Artifacts/EscudoDeGrafito.tres`
- Modify: `Core/Artifacts/NucleoDenso.tres`
- Modify: `Core/Combat/Attacks/EnemyBolt.tres`

**Step 1: EscudoDeGrafito.tres — HealthBonus 0.2 → 10**

Abrir el archivo y cambiar:
```
# Antes:
HealthBonus = 0.2

# Después:
HealthBonus = 10
```

**Step 2: NucleoDenso.tres — HealthBonus 0.25 → 20**

```
# Antes:
HealthBonus = 0.25

# Después:
HealthBonus = 20
```

**Step 3: EnemyBolt.tres — DamageCoefficient 0.8 → 1.0**

El EnemyBolt debe hacer exactamente 1 máscara de daño al player base (AttackStat=10, coeff=1.0 → 10 daño = 1 máscara).

```
# Antes:
DamageCoefficient = 0.8

# Después:
DamageCoefficient = 1.0
```

**Step 4: Verificar build**

```bash
dotnet build
```

**Step 5: Commit**

```bash
git add Core/Artifacts/EscudoDeGrafito.tres \
        Core/Artifacts/NucleoDenso.tres \
        Core/Combat/Attacks/EnemyBolt.tres
git commit -m "chore: actualizar valores de artefactos y EnemyBolt a escala de máscaras"
```

---

### Task 9: Actualizar EntityStats en .tscn

Actualizar `VitalityStat` del Player y BasicEnemy. Eliminar `DefenseStat` de los nodos.

**Files:**
- Modify: `Scenes/Player/Player.tscn`
- Modify: `Scenes/Enemies/BasicEnemy/BasicEnemy.tscn`
- Inspect: `Scenes/Bosses/SoulFragment1/SoulFragment1.tscn` (si existe, actualizar igual)

**Step 1: Player.tscn**

Buscar el bloque de `EntityStats` inline (cerca de la línea 68) y cambiar:
```
# Antes:
VitalityStat = 100
AttackStat = 10
DefenseStat = 10

# Después:
VitalityStat = 40
AttackStat = 10
# DefenseStat: eliminar esta línea
```

**Step 2: BasicEnemy.tscn**

Buscar el bloque de `EntityStats` y cambiar:
```
# Antes:
VitalityStat = 60
AttackStat = 25

# Después:
VitalityStat = 30
AttackStat = 10
# DefenseStat: eliminar si existe
```

**Step 3: SoulFragment1.tscn (si está ensamblado)**

Si el archivo contiene un bloque EntityStats, actualizar `VitalityStat` según el diseño del boss y eliminar `DefenseStat`.

**Step 4: Verificar en Godot**

Abrir el editor Godot (F5 o recargar). Verificar que:
- El Player aparece con 40 HP en el inspector del componente
- El BasicEnemy aparece con 30 HP
- No hay errores en la consola de Godot sobre propiedades inexistentes

**Step 5: Verificar build**

```bash
dotnet build
```

**Step 6: Commit**

```bash
git add Scenes/Player/Player.tscn \
        Scenes/Enemies/BasicEnemy/BasicEnemy.tscn
git commit -m "chore: Player VitalityStat=40, Enemy VitalityStat=30, eliminar DefenseStat de escenas"
```

---

## Verificación final

Con todas las tasks completadas, el sistema debe cumplir:

```
✓ dotnet build → 0 errores, 0 warnings relevantes
✓ Player MaxHealth = 40 (4 máscaras) sin bonuses
✓ Elevation 2 completada → MaxHealth = 50 (5 máscaras)
✓ Escudo de Grafito equipado → MaxHealth += 10
✓ EnemyBolt al player → 10 daño exacto → -1 máscara
✓ Daño de 3 → clampea a 5 (media máscara)
✓ Daño de 8 → clampea a 10 (máscara completa)
✓ DamageMultiplier de resonancias → se aplica al disparar (bug corregido)
✓ DamageResult.cs → no existe en el proyecto
✓ DefenseStat → no existe en EntityStats
```
