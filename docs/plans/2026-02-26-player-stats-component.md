# PlayerStatsComponent Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Crear `PlayerStatsComponent` que hereda de `EntityStatsComponent` y maneja automáticamente los multipliers de progresión y artefactos, eliminando `ApplyAllMultipliers()` de `Player.cs` y las llamadas manuales desde los menús.

**Architecture:** `PlayerStatsComponent` se suscribe a `StatsChanged` events de `ProgressionManager` y `ArtifactManager` en `_Ready()`. Cuando cualquier manager notifica un cambio, el componente recalcula multipliers automáticamente. Los menús dejan de tener responsabilidad sobre las stats.

**Tech Stack:** Godot 4.6, C# .NET 10.0. Sin test suite — verificación: `dotnet build` + F5 en Godot.

**Design doc:** `docs/plans/2026-02-26-player-stats-component-design.md`

---

### Task 1: Agregar `StatsChanged` event a `ProgressionManager`

**Files:**
- Modify: `Core/Progression/ProgressionManager.cs`

**Step 1: Agregar el event y dispararlo en los métodos relevantes**

En `Core/Progression/ProgressionManager.cs`, agregar el event y llamarlo en `ActivateResonance()` y `AdvanceElevation()`:

```csharp
public class ProgressionManager
{
    public event Action StatsChanged;

    // ... propiedades existentes sin cambios ...

    public void ActivateResonance()
    {
        ActivatedResonances++;
        StatsChanged?.Invoke();
    }

    public void AdvanceElevation()
    {
        CurrentElevation++;
        StatsChanged?.Invoke();
    }

    // ... resto sin cambios ...
}
```

Agregar `using System;` si no está presente (para `Action`).

**Step 2: Verificar build**

```bash
dotnet build
```

Esperado: `Build succeeded. 0 Error(s)`

**Step 3: Commit**

```bash
git add Core/Progression/ProgressionManager.cs
git commit -m "feat: agregar StatsChanged event a ProgressionManager"
```

---

### Task 2: Agregar `StatsChanged` event a `ArtifactManager`

**Files:**
- Modify: `Core/Artifacts/ArtifactManager.cs`

**Step 1: Agregar el event y dispararlo en `Equip()` y `Unequip()`**

En `Core/Artifacts/ArtifactManager.cs`, agregar el event:

```csharp
public class ArtifactManager
{
    public event Action StatsChanged;

    // ... resto sin cambios hasta Equip() ...

    public bool Equip(ArtifactType type)
    {
        if (!CanEquip(type))
            return false;

        _equipped.Add(type);
        StatsChanged?.Invoke();
        return true;
    }

    public bool Unequip(ArtifactType type)
    {
        bool removed = _equipped.Remove(type);
        if (removed)
            StatsChanged?.Invoke();
        return removed;
    }

    // ... resto sin cambios ...
}
```

Agregar `using System;` si no está presente.

**Step 2: Verificar build**

```bash
dotnet build
```

Esperado: `Build succeeded. 0 Error(s)`

**Step 3: Commit**

```bash
git add Core/Artifacts/ArtifactManager.cs
git commit -m "feat: agregar StatsChanged event a ArtifactManager"
```

---

### Task 3: Crear `PlayerStatsComponent`

**Files:**
- Create: `Scenes/Player/Components/PlayerStatsComponent.cs`

**Step 1: Crear el archivo**

```csharp
using Godot;
using RotOfTime.Autoload;
using RotOfTime.Core.Entities.Components;

namespace RotOfTime.Scenes.Player.Components;

/// <summary>
///     Extends EntityStatsComponent with automatic multiplier recalculation
///     from ProgressionManager and ArtifactManager. Subscribe/unsuscribe
///     to StatsChanged events from both managers.
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

        float hpMult  = prog?.GetHealthMultiplier() ?? 1.0f;
        float dmgMult = prog?.GetDamageMultiplier() ?? 1.0f;

        hpMult  += arts?.GetTotalHealthBonus() ?? 0f;
        dmgMult += arts?.GetTotalDamageBonus() ?? 0f;

        HealthMultiplier = hpMult;
        DamageMultiplier = dmgMult;
        RecalculateStats();
    }
}
```

**Step 2: Verificar build**

```bash
dotnet build
```

Esperado: `Build succeeded. 0 Error(s)`

**Step 3: Commit**

```bash
git add Scenes/Player/Components/PlayerStatsComponent.cs
git commit -m "feat: crear PlayerStatsComponent con auto-recálculo de multipliers"
```

---

### Task 4: Actualizar `Player.cs`

**Files:**
- Modify: `Scenes/Player/Player.cs`

**Step 1: Cambiar el tipo del export y eliminar `ApplyAllMultipliers()`**

Agregar el using necesario al principio:
```csharp
using RotOfTime.Scenes.Player.Components;
```

Cambiar el export:
```csharp
// Antes:
[Export] public EntityStatsComponent EntityStatsComponent;

// Después:
[Export] public PlayerStatsComponent EntityStatsComponent;
```

En `_Ready()`, eliminar la línea:
```csharp
ApplyAllMultipliers();  // ← eliminar
```

Eliminar el método completo `ApplyAllMultipliers()` (líneas 120-134 aproximadamente):
```csharp
// Eliminar todo este método:
public void ApplyAllMultipliers()
{
    var prog = GameManager.Instance?.ProgressionManager;
    var arts = GameManager.Instance?.ArtifactManager;

    float hpMult = prog?.GetHealthMultiplier() ?? 1.0f;
    float dmgMult = prog?.GetDamageMultiplier() ?? 1.0f;

    hpMult += arts?.GetTotalHealthBonus() ?? 0f;
    dmgMult += arts?.GetTotalDamageBonus() ?? 0f;

    EntityStatsComponent.HealthMultiplier = hpMult;
    EntityStatsComponent.DamageMultiplier = dmgMult;
    EntityStatsComponent.RecalculateStats();
}
```

El `using RotOfTime.Autoload;` puede eliminarse si ya no se usa directamente en `Player.cs`. Verificar que `GameManager` ya no sea referenciado en `Player.cs` fuera de `ApplyAllMultipliers()` antes de eliminarlo.

**Step 2: Verificar build**

```bash
dotnet build
```

Esperado: `Build succeeded. 0 Error(s)`

Si hay error de compilación por referencias a `ApplyAllMultipliers` desde los menús: es esperado — se resuelve en Tasks 6 y 7.

**Step 3: Commit**

```bash
git add Scenes/Player/Player.cs
git commit -m "refactor: eliminar ApplyAllMultipliers de Player.cs"
```

---

### Task 5: Actualizar `Player.tscn`

**Files:**
- Modify: `Scenes/Player/Player.tscn`

Esta tarea se hace en el **editor de Godot**, no editando el .tscn a mano.

**Step 1: Abrir la escena en Godot**

Abrir `Scenes/Player/Player.tscn` en el editor de Godot.

**Step 2: Cambiar el script del nodo EntityStatsComponent**

1. Seleccionar el nodo `EntityStatsComponent` en el panel Scene
2. En el Inspector, en la propiedad `Script`, hacer clic y cambiar de `EntityStatsComponent` a `PlayerStatsComponent`
3. El path correcto es `res://Scenes/Player/Components/PlayerStatsComponent.cs`

**Step 3: Verificar que el Export sigue apuntando correctamente**

Seleccionar el nodo raíz `Player` y verificar en el Inspector que `Entity Stats Component` apunta al nodo `EntityStatsComponent` (no cambió).

**Step 4: Guardar la escena**

`Ctrl+S`

**Step 5: Verificar build**

```bash
dotnet build
```

Esperado: `Build succeeded. 0 Error(s)`

**Step 6: Commit**

```bash
git add Scenes/Player/Player.tscn
git commit -m "refactor: Player.tscn usa PlayerStatsComponent en nodo EntityStatsComponent"
```

---

### Task 6: Actualizar `EquipmentPanel.cs`

**Files:**
- Modify: `Scenes/UI/GlobalMenu/EquipmentPanel/EquipmentPanel.cs`

**Step 1: Eliminar la llamada a `ApplyAllMultipliers()`**

En `Refresh()`, el lambda del `ActionPerformed`:

```csharp
// Antes:
row.ActionPerformed += () =>
{
    player?.ApplyAllMultipliers();  // ← eliminar esta línea
    GameManager.Instance.SaveMeta();
    Refresh();
};
```

```csharp
// Después:
row.ActionPerformed += () =>
{
    GameManager.Instance.SaveMeta();
    Refresh();
};
```

También eliminar la variable `player` si ya no se usa:

```csharp
// Antes:
var player = GetTree().GetFirstNodeInGroup(Groups.Player) as RotOfTime.Scenes.Player.Player;

// Si player no se usa en ningún otro lugar del método, eliminar esta línea
```

Verificar que `player` no tenga otros usos en el método antes de eliminarla. Si `Groups` ya no se usa, se puede eliminar `using RotOfTime.Core.Entities;`.

**Step 2: Verificar build**

```bash
dotnet build
```

Esperado: `Build succeeded. 0 Error(s)`

**Step 3: Commit**

```bash
git add Scenes/UI/GlobalMenu/EquipmentPanel/EquipmentPanel.cs
git commit -m "refactor: eliminar ApplyAllMultipliers de EquipmentPanel"
```

---

### Task 7: Actualizar `ElevationPanel.cs`

**Files:**
- Modify: `Scenes/UI/GlobalMenu/ElevationPanel/ElevationPanel.cs`

**Step 1: Eliminar las dos llamadas a `ApplyAllMultipliers()`**

En `OnActivatePressed()`:
```csharp
// Antes:
prog.ActivateResonance();
var player = GetTree().GetFirstNodeInGroup(Groups.Player) as Player.Player;
player?.ApplyAllMultipliers();  // ← eliminar
GameManager.Instance.SaveMeta();

// Después:
prog.ActivateResonance();
GameManager.Instance.SaveMeta();
```

En `OnElevationPressed()`:
```csharp
// Antes:
prog.AdvanceElevation();
var player = GetTree().GetFirstNodeInGroup(Groups.Player) as Player.Player;
player?.ApplyAllMultipliers();  // ← eliminar
GameManager.Instance.SaveMeta();

// Después:
prog.AdvanceElevation();
GameManager.Instance.SaveMeta();
```

Eliminar también las dos líneas de `var player = GetTree()...` y los usings de `Player` si ya no se usan.

**Step 2: Verificar build**

```bash
dotnet build
```

Esperado: `Build succeeded. 0 Error(s)`

**Step 3: Commit**

```bash
git add Scenes/UI/GlobalMenu/ElevationPanel/ElevationPanel.cs
git commit -m "refactor: eliminar ApplyAllMultipliers de ElevationPanel"
```

---

### Task 8: Verificación final en Godot

**Step 1: Build limpio**

```bash
dotnet build
```

Esperado: `Build succeeded. 0 Warning(s)  0 Error(s)`

**Step 2: Abrir Godot y correr la escena (F5)**

Verificar:
- [ ] El player aparece con HP correcto (barra llena al iniciar)
- [ ] Si hay resonancias activas (en un save existente), el MaxHP refleja los multipliers
- [ ] Abrir ArtifactsMenu (K), equipar/desequipar un artefacto → HP bar actualiza automáticamente sin acción manual
- [ ] Activar una resonancia en BonfireMenu → HP bar actualiza automáticamente
- [ ] El player recibe daño correctamente (combat pipeline no roto)
- [ ] Al morir, `EntityDied` se dispara normalmente

**Step 3: Verificar que no hay calls residuales**

```bash
grep -r "ApplyAllMultipliers" Scenes/ Core/
```

Esperado: sin resultados (solo puede aparecer en docs/).
