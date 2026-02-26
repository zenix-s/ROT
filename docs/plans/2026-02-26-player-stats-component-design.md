# PlayerStatsComponent — Diseño

**Fecha:** 2026-02-26
**Estado:** Aprobado

## Motivación

`Player.cs` actúa como coordinador entre `ProgressionManager`, `ArtifactManager` y `EntityStatsComponent` mediante `ApplyAllMultipliers()`. Esto mezcla responsabilidades en `Player.cs` y requiere que los menús llamen `ApplyAllMultipliers()` manualmente al cambiar equipo o resonancias.

**Objetivos:**
- Mover la coordinación de stats a un componente dedicado
- Hacer que el recálculo sea automático (event-driven), sin llamadas manuales desde menús

## Arquitectura

### Nuevo componente: `PlayerStatsComponent`

**Ubicación:** `Scenes/Player/Components/PlayerStatsComponent.cs`

Hereda de `EntityStatsComponent`. Se suscribe a eventos de los managers y recalcula multipliers automáticamente.

```
PlayerStatsComponent : EntityStatsComponent
  _Ready()
    → base._Ready()  (SetupHealth con mult 1.0)
    → Subscribe(ProgressionManager.StatsChanged)
    → Subscribe(ArtifactManager.StatsChanged)
    → RecalculateFromManagers()
    → ResetStats()   ← CurrentHealth = MaxHealth con multipliers correctos (fix bug inicialización)

  _ExitTree()
    → Unsubscribe ambos events

  RecalculateFromManagers()
    → lee prog + arts de GameManager.Instance
    → setea HealthMultiplier / DamageMultiplier
    → RecalculateStats()
```

### Eventos en managers

`ProgressionManager` y `ArtifactManager` agregan `event Action StatsChanged`:

```
ProgressionManager.StatsChanged  →  fire en: ActivateResonance(), AdvanceElevation()
ArtifactManager.StatsChanged     →  fire en: Equip(), Unequip()
```

Patrón ya establecido por `EconomyManager.IsotopesChanged`.

## Flujo de datos

### Inicialización

```
PlayerStatsComponent._Ready()   (corre antes que Player._Ready() — hijos primero)
  → base._Ready() → CurrentHealth = MaxHealth (mult 1.0)
  → suscripción a events
  → RecalculateFromManagers() → multipliers correctos → RecalculateStats()
  → ResetStats() → CurrentHealth = MaxHealth con multipliers finales
```

### Runtime (equip artefacto / activar resonancia)

```
ArtifactManager.Equip(type)
  → StatsChanged?.Invoke()
      → PlayerStatsComponent.RecalculateFromManagers()
          → RecalculateStats() → EmitSignal(StatsUpdated)
```

Los menús ya no necesitan llamar `ApplyAllMultipliers()`.

### Fórmula de multipliers (sin cambios)

```csharp
float hpMult  = prog.GetHealthMultiplier()  + arts.GetTotalHealthBonus();
float dmgMult = prog.GetDamageMultiplier()  + arts.GetTotalDamageBonus();
```

## Cambios en Player.cs

- Eliminar `ApplyAllMultipliers()`
- Cambiar tipo del export: `EntityStatsComponent` → `PlayerStatsComponent`
- Eliminar llamada en `_Ready()` y en `EquipDash()` (el dash no afecta stats)

## Archivos afectados

| Archivo | Cambio |
|---|---|
| `Core/Progression/ProgressionManager.cs` | `event Action StatsChanged`, fire en `ActivateResonance()` y `AdvanceElevation()` |
| `Core/Artifacts/ArtifactManager.cs` | `event Action StatsChanged`, fire en `Equip()` y `Unequip()` |
| `Scenes/Player/Components/PlayerStatsComponent.cs` | **Crear** |
| `Scenes/Player/Player.cs` | Eliminar `ApplyAllMultipliers()`, cambiar tipo export, limpiar llamadas |
| `Scenes/Player/Player.tscn` | Cambiar script del nodo `EntityStatsComponent` a `PlayerStatsComponent` |
| Menús con llamadas a `ApplyAllMultipliers()` | Eliminar esas llamadas |

## Nota futura

`RecalculateStats()` actualmente solo clampea `CurrentHealth` hacia abajo. Si en el futuro se implementa persistencia de run-state (guardar HP entre salas), `ResetStats()` en `_Ready()` deberá reemplazarse por restaurar el HP guardado.
