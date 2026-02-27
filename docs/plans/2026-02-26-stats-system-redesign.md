# Stats System Redesign — Design Doc
**Fecha:** 2026-02-26

## Problema

El sistema de estadísticas actual tiene cuatro defectos fundamentales:

1. **`VitalityStat = 100` sin semántica** — no comunica cuántos golpes aguanta la entidad.
2. **`DefenseStat` como resta plana** — `daño - defensa` se vuelve irrelevante en late game y no escala.
3. **`HealthMultiplier` flotante** — `100 * 1.6f = 160`, número sin significado para el diseñador ni el jugador.
4. **Bug: `DamageMultiplier` nunca se aplica al daño del jugador** — `Player.TryFireAttack()` pasa `EntityStatsComponent.EntityStats` (crudo, sin multiplicadores) al `AttackManager`. Los bonuses de resonancias y artefactos al daño son actualmente nulos.

## Decisiones de Diseño

### Unidad: 1 máscara = 10 HP

Todos los valores de vida están diseñados en múltiplos de 10. El display muestra `CurrentHealth / 10` como conteo de máscaras. Los ataques dirigidos al jugador se diseñan con `DamageCoefficient` tal que `AttackStat(10) × coeff` sea múltiplo de 5.

- **Máscaras completas:** vida máxima siempre múltiplo de 10.
- **Medias máscaras:** el daño puede ser múltiplo de 5 (5 = media máscara, 10 = máscara completa).

### Progresión de vida

La vida sube con **Elevaciones completadas**, no con Resonancias:

| Estado | MaxHealth | Máscaras |
|--------|-----------|----------|
| Elevación 1 (inicio) | 40 | 4 |
| Elevación 2 completada | 50 | 5 |
| Elevación 3 completada | 60 | 6 |
| Elevación 4 completada | 70 | 7 |
| Elevación 5 completada | 80 | 8 |

Las **Resonancias** solo afectan al multiplicador de daño (`+10%` por resonancia, igual que antes).

### Defensa eliminada

Sin `DefenseStat`. La defensa es esquivar, usar i-frames del dash. El daño de cada ataque es predecible.

### Enforcement interno en los Resources

En lugar de un enforcer externo, las invariantes viven en los propios Resources:

**`AttackResult`** — el constructor clampea `RawDamage` a múltiplo de 5 con threshold 3:
- 0–2 → 0, 3–4 → 5, 5–7 → 5, 8–9 → 10, etc.

**`EntityStats`** — el setter de `VitalityStat` clampea a múltiplo de 10:
- 45 → 40, 55 → 50, etc.

## Arquitectura Resultante

### Qué se elimina

| Elemento | Razón |
|----------|-------|
| `EntityStats.DefenseStat` | Defensa = esquivar |
| `EntityStatsComponent.HealthMultiplier` (float) | Reemplazado por `MaxHealthBonus` (int) |
| `ProgressionManager.GetHealthMultiplier()` | Reemplazado por `GetHealthBonus()` |
| `DamageCalculator.CalculateFinalDamage()` | Sin defensa, sin cálculo de reducción |
| `DamageResult.cs` (record) | Solo lo usaba `CalculateFinalDamage` |

### Qué cambia

**`EntityStats`**
- Eliminar `DefenseStat`
- `VitalityStat` setter: clampea a múltiplo de 10
- Player: `VitalityStat = 40`, Enemy: `VitalityStat = 30`

**`AttackResult`**
- Constructor: clampea `RawDamage` a múltiplo de 5 con threshold 3

**`EntityStatsComponent`**
- `HealthMultiplier` (float) → `MaxHealthBonus` (int)
- `MaxHealth = EntityStats.VitalityStat + MaxHealthBonus`
- `TakeDamage()`: resta `attackResult.RawDamage` directamente, sin `DamageCalculator.CalculateFinalDamage`

**`PlayerStatsComponent`**
- `MaxHealthBonus = progression.GetHealthBonus() + artifacts.GetTotalHealthBonus()`
- `DamageMultiplier` sin cambios (sigue siendo float aditivo)

**`ProgressionManager`**
- Eliminar `GetHealthMultiplier()`
- Añadir `GetHealthBonus() => (CurrentElevation - 1) * 10`

**`ArtifactData`**
- `HealthBonus`: `float` → `int` (10 = +1 máscara)

**`ArtifactManager`**
- `GetTotalHealthBonus()`: retorna `int`

**Bug fix — `Player.TryFireAttack()`**
- Pasar `EntityStatsComponent.DamageMultiplier` al `AttackManager`
- Propagarlo hasta `AttackHitboxComponent.Initialize()`

**`DamageCalculator`**
- Eliminar `CalculateFinalDamage(AttackResult, EntityStats)`
- `CalculateRawDamage` acepta `damageMultiplier` como parámetro explícito

### Valores nuevos en `.tres` / `.tscn`

| Recurso | Propiedad | Antes | Después |
|---------|-----------|-------|---------|
| `Player.tscn` | `VitalityStat` | 100 | **40** |
| `Player.tscn` | `DefenseStat` | 10 | **eliminado** |
| `BasicEnemy.tscn` | `VitalityStat` | 60 | **30** |
| `BasicEnemy.tscn` | `DefenseStat` | 0 | **eliminado** |
| `EnemyBolt.tres` | `DamageCoefficient` | 0.8 | **1.0** |
| `EscudoDeGrafito.tres` | `HealthBonus` | 0.20f | **10** |
| `NucleoDenso.tres` | `HealthBonus` | 0.25f | **20** |

### Sistema resultante end-to-end

```
Player base:               VitalityStat=40 → 4 máscaras
Elevation 3 completada:    MaxHealthBonus=20 → MaxHealth=60 → 6 máscaras
Escudo de Grafito equipado: MaxHealthBonus += 10 → MaxHealth=70 → 7 máscaras

EnemyBolt al player:       AttackStat=10 × coeff=1.0 = 10 → -1 máscara exacta
RockBody al player:        AttackStat=10 × coeff=1.0 = 10 → -1 máscara exacta

Player CarbonBolt:         AttackStat=10 × DamageMultiplier=1.3 × coeff=1.0 → 13 daño al enemigo
BasicEnemy HP:             VitalityStat=30 → muere en 3 hits base
```

## Archivos a Modificar

```
Core/Entities/EntityStats.cs
Core/Entities/Components/EntityStatsComponent.cs
Core/Progression/ProgressionManager.cs
Core/Artifacts/ArtifactData.cs
Core/Artifacts/ArtifactManager.cs
Core/Combat/Results/AttackResult.cs
Core/Combat/Results/DamageResult.cs                  ← ELIMINAR
Core/Combat/Calculations/DamageCalculator.cs
Core/Combat/Components/AttackHitboxComponent.cs
Core/Combat/Components/AttackManagerComponent.cs     ← propagar damageMultiplier
Scenes/Player/Player.cs                              ← bug fix damageMultiplier
Scenes/Player/Components/PlayerStatsComponent.cs
Core/Artifacts/EscudoDeGrafito.tres
Core/Artifacts/NucleoDenso.tres
Core/Combat/Attacks/EnemyBolt.tres
Scenes/Player/Player.tscn                            ← VitalityStat, eliminar DefenseStat
Scenes/Enemies/BasicEnemy/BasicEnemy.tscn            ← VitalityStat, eliminar DefenseStat
```
