# Tareas Pendientes — Rot of Time

> Backlog general de trabajo pendiente. Revisar al inicio de cada sesión.
> Para planes de implementación detallados ver `docs/plans/`.

---

## En el cajón

### Código

- [ ] **Brainstorming: pipeline de resonancias y elevación**
  Falta: `ResonanceTrigger` scene (Area2D reutilizable), conexión resonancia→`Player.ApplyAllMultipliers()`, y lógica de avance de elevación al derrotar boss.
  Ver análisis en conversación 2026-02-18.

- [ ] **Bloque C: Artifact Menu**
  Brainstorming pendiente antes de implementar. Implica: método `Craft()` en ArtifactManager, UI de inventario (equip/unequip), panel de crafteo con coste en isótopos.

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
