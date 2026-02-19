# Tareas Pendientes — Rot of Time

> Backlog general de trabajo pendiente. Revisar al inicio de cada sesión.
> Para planes de implementación detallados ver `docs/plans/`.

---

## En el cajón

### Código

- [x] **Pipeline de resonancias y elevación** (2026-02-19)
  InventoryManager, ProgressionManager simplificado, ResonanceTrigger, ElevationItem, BonfireMenu, Bonfire. Pendiente: input action `interact` (tecla E) en Godot editor + test manual.

- [ ] **Bloque C-1: Panel Artefactos en Bonfire**
  Panel equip/unequip dentro de `BonfireMenu`. Lista de artifacts poseídos, contador de slots, botones Equipar/Desequipar. Llama `Player.ApplyAllMultipliers()` y `SaveMeta()` al cambiar.

- [ ] **Bloque C-2: Panel Crafteo en Bonfire**
  Panel de crafteo dentro de `BonfireMenu`. Lista de artifacts crafteables (hardcoded, los `.tres` existentes) con coste en isótopos. Requiere `[Export] int IsotopeCost` en `ArtifactData` y `ArtifactManager.AddOwned()`. Llama `EconomyManager.TrySpend()` y `SaveMeta()` al craftear.

- [ ] **Lore: descripciones de Elevación**
  Añadir `ElevationData` Resource con nombre y descripción de lore por elevación (1-5). Mostrar en BonfireMenu al ascender. Da contexto narrativo al sistema de progresión.

- [ ] **Refactor: isótopos a InventoryManager**
  Mover isótopos de `EconomyManager` a `InventoryManager` para unificar todos los coleccionables en un solo lugar. Actualizar HUD, BonfireMenu (crafteo), y `MetaData`. No bloquea nada actualmente — hacer cuando haya tiempo.

- [ ] **Bloque D: Save/Load**
  Auditar `MetaData.cs`, añadir trigger de guardado al morir y al volver al menú, verificar que `LoadMeta()` restaura estado completo.

### Manual en Godot (developer)

- [ ] **Bloque E-1: Floor 1** — TileMap grey-box, 3-5 enemies, trigger de Resonancia
- [ ] **Bloque E-2: Floor 2 y Floor 3**
- [ ] **Bloque E-3: Boss 1 (Soul Fragment 1)** — state machine de fases
- [ ] **Bloque E-4: Arena del boss** — puerta bloqueada + trigger de Elevation 2

---

## Completado

- [x] Phase 1: Refactor sistema de ataques
- [x] Phase 2: Progression System (Elevations & Resonances — datos y lógica)
- [x] Phase 2: Artifact System (slots, equip/unequip, stat modifiers)
- [x] Phase 3: Spells (Carbon Bolt, Fireball, Ice Shard burst)
- [x] Phase 4: Enemy AI (BasicEnemy con chase + AttackingState + RockBody)
- [x] Phase 4: Isotope drop system (EconomyManager + IsotopePickup)
- [x] Bloque B: HUD (HP bar, contador isótopos, 3 cooldown indicators)
