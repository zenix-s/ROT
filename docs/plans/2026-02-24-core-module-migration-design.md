# Diseño: Migración de managers de `Autoload/` a `Core/`

**Fecha:** 2026-02-24
**Estado:** Aprobado

## Objetivo

Mover los plain C# managers que viven en `Autoload/` a sus módulos correspondientes en `Core/`, siguiendo el patrón de `Core/Dash/` (cada sistema tiene su propia carpeta con sus archivos).

## Motivación

Los managers como `SaveManager`, `GameStateManager`, `ProgressionManager` e `InventoryManager` son parte de sistemas de dominio independientes — no tienen razón arquitectónica para estar en `Autoload/`. Solo son Godot Nodes los dos autoloads reales: `GameManager` y `SceneManager`.

## Qué se mueve

| Archivo actual | Destino | Namespace nuevo |
|---|---|---|
| `Autoload/SaveManager.cs` | `Core/GameData/SaveManager.cs` | `RotOfTime.Core.GameData` |
| `Autoload/GameStateManager.cs` | `Core/GameState/GameStateManager.cs` | `RotOfTime.Core.GameState` |
| `Autoload/ProgressionManager.cs` | `Core/Progression/ProgressionManager.cs` | `RotOfTime.Core.Progression` |
| `Autoload/InventoryManager.cs` | `Core/Inventory/InventoryManager.cs` | `RotOfTime.Core.Inventory` |

Se crean 2 carpetas nuevas: `Core/GameState/` y `Core/Inventory/`.

## Qué se queda en `Autoload/`

- `GameManager.cs` — Godot Node registrado como autoload en `project.godot`
- `SceneManager.cs` — Godot Node registrado como autoload en `project.godot`
- `AbilityManager.cs` — stub, dep circular a `PlayerAttackSlot` de `Scenes/Player/`

## Impacto en otros archivos

**`Autoload/GameManager.cs`:**
- Añadir `using RotOfTime.Core.GameData`, `using RotOfTime.Core.GameState`, `using RotOfTime.Core.Progression`, `using RotOfTime.Core.Inventory`
- Eliminar usings de `RotOfTime.Autoload` para los tipos movidos (ya no son necesarios, están en el mismo archivo)

**`Core/GameData/MetaData.cs`:**
- Usa `Milestone` (enum definido en `GameStateManager.cs`)
- Reemplazar `using RotOfTime.Autoload` con `using RotOfTime.Core.GameState`

**Resto de archivos con `using RotOfTime.Autoload`:**
- Sin cambios. Todos acceden a `GameManager`/`SceneManager`, que permanecen en `RotOfTime.Autoload`.

## Estructura final

```
Autoload/
  GameManager.cs       ← Godot Node, orquestador
  SceneManager.cs      ← Godot Node, transiciones
  AbilityManager.cs    ← stub

Core/
  GameData/
    MetaData.cs
    SaveManager.cs     ← movido desde Autoload/
  GameState/
    GameStateManager.cs ← movido desde Autoload/ (carpeta nueva)
  Progression/
    ProgressionManager.cs ← movido desde Autoload/
    ElevationItem.cs
    ResonanceTrigger.cs
  Inventory/
    InventoryManager.cs ← movido desde Autoload/ (carpeta nueva)
  Artifacts/ Economy/ Dash/ Combat/ Entities/ ...  (sin cambios)
```

## No en scope

- Mover `GameManager.cs` o `SceneManager.cs`
- Mover `AbilityManager.cs` (depende de resolver la dependencia circular primero)
- Reorganizar `Core/` root (`GameConstants.cs`, `SceneExtensionManager.cs`)
