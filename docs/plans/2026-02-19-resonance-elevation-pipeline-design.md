# Diseño: Pipeline de Resonancias y Elevación

**Fecha:** 2026-02-19
**Estado:** Aprobado

---

## Resumen

Las resonancias son ítems fungibles que el jugador recoge en el mundo y activa en la bonfire. Los elevation items son drops del boss que permiten avanzar de elevación. Todo lo coleccionable (excepto isótopos, pendiente de refactor) vive en `InventoryManager`.

Filosofía: el jugador controla cuándo activar. Permite runs de bajo nivel (sin activar resonancias).

---

## Componentes

| Componente | Tipo | Responsabilidad |
|---|---|---|
| `InventoryManager` | Clase C# plana (owned by GameManager) | `Dictionary<string,int>` de todos los ítems coleccionables |
| `ProgressionManager` (simplificado) | Clase C# plana | Contador de resonancias activadas + elevación actual |
| `ResonanceTrigger` | Area2D (scene colocable en niveles) | Pickup → `AddItem("resonance")` |
| `ElevationItem` | Area2D (drop del boss) | Pickup → `AddItem("elevation_N")` |
| `BonfireMenu` | CanvasLayer (hijo de Main.tscn) | UI para activar resonancias y ascender |
| `Bonfire` | Node2D (scene colocable en niveles) | Detecta player + tecla interact → abre BonfireMenu |

---

## Capa de datos

### MetaData (cambios)

```csharp
// Eliminar:
public List<string> UnlockedResonances { get; set; } = [];

// Añadir:
public int ActivatedResonances { get; set; }
public Dictionary<string, int> Inventory { get; set; } = new();
```

### InventoryManager (nuevo)

```
Dictionary<string, int>
  "resonance"    → N  (recogidas, no activadas)
  "elevation_1"  → 1  (drop del boss de elevación 1)
  "elevation_2"  → 1  (drop del boss de elevación 2)
  ...
```

API: `AddItem(id, amount=1)`, `RemoveItem(id, amount=1) → bool`, `HasItem(id, amount=1) → bool`, `GetQuantity(id) → int`

### ProgressionManager (simplificado)

```
int ActivatedResonances  (contador global, no por elevación)
int CurrentElevation

GetHealthMultiplier() → 1.0 + ActivatedResonances * 0.20
GetDamageMultiplier() → 1.0 + ActivatedResonances * 0.10
ActivateResonance()   → ActivatedResonances++
AdvanceElevation()    → CurrentElevation++
```

Eliminado: `HashSet<string> _unlockedResonances`, `UnlockResonance(int,int)`, `IsResonanceUnlocked()`, `GetResonanceCount()`, `IsElevationComplete()`, `CountAllResonances()`, `GetResonanceKeys()`, `LoadResonanceKeys()`.

---

## Flujo de datos

```
[RECOGER RESONANCIA]
ResonanceTrigger (body_entered)
  → InventoryManager.AddItem("resonance")
  → GameManager.SaveMeta()
  → QueueFree()

[RECOGER ELEVATION ITEM]
ElevationItem (body_entered)
  → InventoryManager.AddItem("elevation_1")
  → GameManager.SaveMeta()
  → QueueFree()

[ACTIVAR EN BONFIRE]
BonfireMenu → "Activar Resonancia"
  → InventoryManager.RemoveItem("resonance")
  → ProgressionManager.ActivateResonance()
  → Player.ApplyAllMultipliers()
  → GameManager.SaveMeta()

[ASCENDER EN BONFIRE]
BonfireMenu → "Ascender a Elevación 2"
  → InventoryManager.RemoveItem("elevation_1")
  → ProgressionManager.AdvanceElevation()
  → Player.ApplyAllMultipliers()
  → GameManager.SaveMeta()
```

---

## BonfireMenu — estructura UI

```
BonfireMenu (CanvasLayer, layer=10, ProcessMode=Always)
  └── Panel (centrado, 400x400)
      └── VBoxContainer (margins 20px)
          ├── Label "☽ Bonfire"            [título]
          ├── HSeparator
          ├── Label "Resonancias"          [sección]
          ├── Label ResonancesLabel        ["Disponibles: N"]
          ├── Button ActivateButton        ["Activar Resonancia"]  disabled si N=0
          ├── HSeparator
          ├── Label ElevationLabel         ["Ascender a Elevación N+1"]  oculto si no hay item
          ├── Button ElevationButton       ["Ascender"]  oculto si no hay item
          ├── HSeparator
          └── Button CloseButton           ["Cerrar"]
```

Pausa: `GetTree().Paused = true` al abrir. BonfireMenu usa `ProcessMode = Always`.

---

## Integración con Main

- BonfireMenu vive como hijo de `Main/UI` (CanvasLayer existente o nuevo).
- `Main.cs` exporta `BonfireMenu _bonfireMenu` y llama `_bonfireMenu.Initialize(_player)` al cargar escena de juego.
- En `OnMenuChangeRequested`: `_bonfireMenu.ForceClose()` para limpiar estado y descongelar árbol.
- Bonfire (en escena de nivel) encuentra BonfireMenu via grupo `"BonfireMenu"` y llama `Open()`.

---

## Input action requerida

Nueva acción `interact` en Input Map de Godot → tecla `E`.
Añadir en: Project > Project Settings > Input Map.

---

## Pendiente (fuera de este plan)

- Panel Artefactos en Bonfire (Bloque C-1)
- Panel Crafteo en Bonfire (Bloque C-2)
- Lore descriptions de elevaciones (`ElevationData` Resource)
- Refactor isótopos a InventoryManager
