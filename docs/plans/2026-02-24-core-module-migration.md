# Core Module Migration Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Mover los plain C# managers de `Autoload/` a sus módulos correspondientes en `Core/`, actualizando namespaces y referencias.

**Architecture:** Cada manager pasa a vivir en la carpeta de su sistema (`Core/GameData/`, `Core/GameState/`, `Core/Progression/`, `Core/Inventory/`). `GameManager.cs` y `SceneManager.cs` quedan en `Autoload/` por ser Godot Nodes registrados en `project.godot`. Cada tarea es atómica: mueve un archivo + actualiza sus consumidores + buildea.

**Tech Stack:** Godot 4.6, C# (.NET 10), sin test suite — verificación via `dotnet build`

---

### Task 1: Mover SaveManager → Core/GameData/

**Files:**
- Delete: `Autoload/SaveManager.cs` (y `Autoload/SaveManager.cs.uid`)
- Create: `Core/GameData/SaveManager.cs`
- No changes en consumidores (`GameManager.cs` ya tiene `using RotOfTime.Core.GameData`)

**Step 1: Crear Core/GameData/SaveManager.cs con namespace actualizado**

Contenido idéntico al original salvo la línea de namespace:

```csharp
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using Godot;
using RotOfTime.Core.GameData;

namespace RotOfTime.Core.GameData;

/// <summary>
///     Persistence layer for meta-progression.
///     Handles file I/O only - no game state management.
/// </summary>
public class SaveManager
{
    private string GetSaveDirectory() => ProjectSettings.GlobalizePath("user://saves/");
    private string GetMetaPath() => Path.Combine(GetSaveDirectory(), "meta.json");

    public SaveManager()
    {
        Directory.CreateDirectory(GetSaveDirectory());
    }

    public bool SaveMeta(MetaData data)
    {
        if (data == null)
        {
            GD.PrintErr("SaveManager: Cannot save null MetaData");
            return false;
        }

        try
        {
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(GetMetaPath(), json, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            GD.Print("SaveManager: Meta saved");
            return true;
        }
        catch (Exception e)
        {
            GD.PrintErr($"SaveManager: Failed to save meta - {e.Message}");
            return false;
        }
    }

    public MetaData LoadMeta()
    {
        string path = GetMetaPath();
        if (!File.Exists(path))
            return null;

        try
        {
            string json = File.ReadAllText(path, Encoding.UTF8);
            MetaData data = JsonSerializer.Deserialize<MetaData>(json);
            GD.Print("SaveManager: Meta loaded");
            return data;
        }
        catch (Exception e)
        {
            GD.PrintErr($"SaveManager: Failed to load meta - {e.Message}");
            return null;
        }
    }
}
```

Nota: el `using RotOfTime.Core.GameData;` queda redundante (misma clase está en ese namespace) pero no rompe. Se puede dejar o quitar — quitar es más limpio.

Versión limpia sin el using redundante:

```csharp
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using Godot;

namespace RotOfTime.Core.GameData;

// ... resto igual
```

**Step 2: Eliminar archivos de Autoload/**

```bash
rm Autoload/SaveManager.cs Autoload/SaveManager.cs.uid
```

**Step 3: Verificar build**

```bash
dotnet build
```

Expected: Build succeeded, 0 errors.

**Step 4: Commit**

```bash
git add Core/GameData/SaveManager.cs Autoload/SaveManager.cs Autoload/SaveManager.cs.uid
git commit -m "refactor: mover SaveManager a Core/GameData"
```

---

### Task 2: Mover GameStateManager → Core/GameState/

**Files:**
- Delete: `Autoload/GameStateManager.cs` (y `.uid`)
- Create: `Core/GameState/GameStateManager.cs`
- Modify: `Autoload/GameManager.cs` — añadir using
- Modify: `Core/GameData/MetaData.cs` — reemplazar using

**Step 1: Crear carpeta Core/GameState/ y el archivo con namespace actualizado**

`Core/GameState/GameStateManager.cs`:

```csharp
using System.Collections.Generic;
using RotOfTime.Autoload;

namespace RotOfTime.Core.GameState;

#region Milestones management

public enum Milestone
{
    GameStarted,
    TutorialStarted
}

public static class MilestoneExtensions
{
    public static bool IsCompleted(this Milestone milestone)
    {
        return GameManager.Instance.GameStateManager.CompletedMilestones.Contains(milestone);
    }

    public static void Complete(this Milestone milestone)
    {
        GameManager.Instance.GameStateManager.CompleteMilestone(milestone);
    }
}

public static class GameStateManagerMilestoneExtensions
{
    extension(GameStateManager manager)
    {
        public void CompleteMilestone(Milestone milestone)
        {
            manager.CompletedMilestones.Add(milestone);
        }

        public void LoadMilestones(IEnumerable<Milestone> milestones)
        {
            manager.CompletedMilestones = new HashSet<Milestone>(milestones);
        }
    }
}

#endregion

public class GameStateManager
{
    public HashSet<Milestone> CompletedMilestones { get; set; } = [];
}
```

Nota: se añade `using RotOfTime.Autoload` porque `MilestoneExtensions` accede a `GameManager.Instance`.

**Step 2: Eliminar archivos de Autoload/**

```bash
rm Autoload/GameStateManager.cs Autoload/GameStateManager.cs.uid
```

**Step 3: Actualizar Autoload/GameManager.cs — añadir using**

En `Autoload/GameManager.cs`, añadir `using RotOfTime.Core.GameState;` junto a los otros usings de Core:

```csharp
using System.Linq;
using Godot;
using RotOfTime.Core;
using RotOfTime.Core.Artifacts;
using RotOfTime.Core.Dash;
using RotOfTime.Core.Economy;
using RotOfTime.Core.GameData;
using RotOfTime.Core.GameState;   // ← añadir
```

**Step 4: Actualizar Core/GameData/MetaData.cs — reemplazar using**

Reemplazar:
```csharp
using RotOfTime.Autoload;
```
Con:
```csharp
using RotOfTime.Core.GameState;
```

**Step 5: Verificar build**

```bash
dotnet build
```

Expected: Build succeeded, 0 errors.

**Step 6: Commit**

```bash
git add Core/GameState/GameStateManager.cs Autoload/GameStateManager.cs Autoload/GameStateManager.cs.uid Autoload/GameManager.cs Core/GameData/MetaData.cs
git commit -m "refactor: mover GameStateManager a Core/GameState"
```

---

### Task 3: Mover ProgressionManager → Core/Progression/

**Files:**
- Delete: `Autoload/ProgressionManager.cs` (y `.uid`)
- Create: `Core/Progression/ProgressionManager.cs`
- Modify: `Autoload/GameManager.cs` — añadir using

**Step 1: Crear Core/Progression/ProgressionManager.cs con namespace actualizado**

```csharp
using Godot;

namespace RotOfTime.Core.Progression;

/// <summary>
///     Tracks activated resonances and current elevation.
///     Plain C# class owned by GameManager (not a Godot Node).
/// </summary>
public class ProgressionManager
{
    public int CurrentElevation { get; set; } = 1;
    public int ActivatedResonances { get; private set; }

    public void ActivateResonance()
    {
        ActivatedResonances++;
    }

    /// <summary>True when the player has activated 3 resonances in the current elevation.</summary>
    public bool CanAdvanceElevation() => ActivatedResonances >= CurrentElevation * 3;

    public void AdvanceElevation()
    {
        CurrentElevation++;
    }

    public float GetHealthMultiplier() => 1.0f + ActivatedResonances * 0.20f;
    public float GetDamageMultiplier() => 1.0f + ActivatedResonances * 0.10f;

    public void Load(int activatedResonances, int currentElevation)
    {
        ActivatedResonances = activatedResonances;
        CurrentElevation = currentElevation;
    }
}
```

**Step 2: Eliminar archivos de Autoload/**

```bash
rm Autoload/ProgressionManager.cs Autoload/ProgressionManager.cs.uid
```

**Step 3: Actualizar Autoload/GameManager.cs — añadir using**

Añadir `using RotOfTime.Core.Progression;`:

```csharp
using System.Linq;
using Godot;
using RotOfTime.Core;
using RotOfTime.Core.Artifacts;
using RotOfTime.Core.Dash;
using RotOfTime.Core.Economy;
using RotOfTime.Core.GameData;
using RotOfTime.Core.GameState;
using RotOfTime.Core.Progression;   // ← añadir
```

**Step 4: Verificar build**

```bash
dotnet build
```

Expected: Build succeeded, 0 errors.

**Step 5: Commit**

```bash
git add Core/Progression/ProgressionManager.cs Autoload/ProgressionManager.cs Autoload/ProgressionManager.cs.uid Autoload/GameManager.cs
git commit -m "refactor: mover ProgressionManager a Core/Progression"
```

---

### Task 4: Mover InventoryManager → Core/Inventory/

**Files:**
- Delete: `Autoload/InventoryManager.cs` (y `.uid`)
- Create: `Core/Inventory/InventoryManager.cs`
- Modify: `Autoload/GameManager.cs` — añadir using

**Step 1: Crear carpeta Core/Inventory/ y el archivo con namespace actualizado**

`Core/Inventory/InventoryManager.cs`:

```csharp
using System.Collections.Generic;

namespace RotOfTime.Core.Inventory;

/// <summary>
///     Manages all collectible items as a Dictionary&lt;string, int&gt;.
///     Plain C# class owned by GameManager (not a Godot Node).
///
///     Known item IDs:
///       "resonance" — collected resonance (fungible, activatable at bonfire)
///       "elevation" — elevation item dropped by any boss (genérico, no por elevación específica)
/// </summary>
public class InventoryManager
{
    private Dictionary<string, int> _items = new();

    public void AddItem(string id, int amount = 1)
    {
        _items.TryGetValue(id, out int current);
        _items[id] = current + amount;
    }

    public bool RemoveItem(string id, int amount = 1)
    {
        if (!HasItem(id, amount)) return false;
        _items[id] -= amount;
        if (_items[id] <= 0) _items.Remove(id);
        return true;
    }

    public bool HasItem(string id, int amount = 1)
    {
        return _items.TryGetValue(id, out int count) && count >= amount;
    }

    public int GetQuantity(string id)
    {
        return _items.TryGetValue(id, out int count) ? count : 0;
    }

    public Dictionary<string, int> GetAllItems() => new(_items);

    public void Load(Dictionary<string, int> items)
    {
        _items = items != null ? new Dictionary<string, int>(items) : new Dictionary<string, int>();
    }
}
```

**Step 2: Eliminar archivos de Autoload/**

```bash
rm Autoload/InventoryManager.cs Autoload/InventoryManager.cs.uid
```

**Step 3: Actualizar Autoload/GameManager.cs — añadir using**

Añadir `using RotOfTime.Core.Inventory;`:

```csharp
using System.Linq;
using Godot;
using RotOfTime.Core;
using RotOfTime.Core.Artifacts;
using RotOfTime.Core.Dash;
using RotOfTime.Core.Economy;
using RotOfTime.Core.GameData;
using RotOfTime.Core.GameState;
using RotOfTime.Core.Inventory;     // ← añadir
using RotOfTime.Core.Progression;
```

**Step 4: Verificar build**

```bash
dotnet build
```

Expected: Build succeeded, 0 errors.

**Step 5: Commit**

```bash
git add Core/Inventory/InventoryManager.cs Autoload/InventoryManager.cs Autoload/InventoryManager.cs.uid Autoload/GameManager.cs
git commit -m "refactor: mover InventoryManager a Core/Inventory"
```

---

### Task 5: Verificación final y actualizar documentación

**Step 1: Confirmar estado de Autoload/**

```bash
ls Autoload/
```

Expected: solo `AbilityManager.cs`, `AbilityManager.cs.uid`, `GameManager.cs`, `GameManager.cs.uid`, `SceneManager.cs`, `SceneManager.cs.uid`

**Step 2: Build limpio final**

```bash
dotnet build
```

Expected: Build succeeded, 0 errors, 0 warnings relevantes.

**Step 3: Actualizar MEMORY.md**

En `~/.claude/projects/-home-zenix-development-ROT/memory/MEMORY.md`, actualizar la sección "Non-Node managers":

```markdown
- **Non-Node managers**: ProgressionManager, ArtifactManager, EconomyManager, DashManager, SaveManager, GameStateManager, InventoryManager son plain C# classes owned by GameManager — not Nodes, not Autoloads. Acceso via `GameManager.Instance.XxxManager`. Viven en sus módulos Core: `Core/Progression/`, `Core/GameData/`, `Core/GameState/`, `Core/Inventory/`, `Core/Economy/`, `Core/Artifacts/`, `Core/Dash/`
```

**Step 4: Actualizar CLAUDE.md — sección Architecture**

En `CLAUDE.md`, la sección "Autoload Singletons" menciona los managers como si fueran parte de `Autoload/`. Actualizar los bullets para reflejar sus nuevas ubicaciones.

**Step 5: Commit final**

```bash
git add CLAUDE.md
git commit -m "docs: actualizar CLAUDE.md con nuevas ubicaciones de managers"
```
