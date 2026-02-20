# Tareas Pendientes — Rot of Time

> Backlog general de trabajo pendiente. Revisar al inicio de cada sesión.
> Para planes de implementación detallados ver `docs/plans/`.

---

## En el cajón

### Código


- [ ] **Sistema de ataques cuerpo a cuerpo** *(a futuro — post-vertical-slice)*
  El `RockBody` fue reemplazado por proyectiles (`EnemyBolt`). Un ataque melee real (para un tipo de enemigo diferente) debería: aparecer brevemente en la dirección del enemigo, tener hitbox activa solo durante el swing, y desaparecer. No hacer hasta tener Level Design + Boss funcionando.

- [ ] **Bloque C-1: Panel Artefactos en Bonfire**
  Panel equip/unequip dentro de `BonfireMenu`. Lista de artifacts poseídos, contador de slots, botones Equipar/Desequipar. Llama `Player.ApplyAllMultipliers()` y `SaveMeta()` al cambiar.

- [ ] **Bloque C-2: Panel Crafteo en Bonfire**
  Panel de crafteo dentro de `BonfireMenu`. Lista de artifacts crafteables (hardcoded, los `.tres` existentes) con coste en isótopos. Requiere `[Export] int IsotopeCost` en `ArtifactData` y `ArtifactManager.AddOwned()`. Llama `EconomyManager.TrySpend()` y `SaveMeta()` al craftear.

- [ ] **Lore: descripciones de Elevación**
  Añadir `ElevationData` Resource con nombre y descripción de lore por elevación (1-5). Mostrar en BonfireMenu al ascender. Da contexto narrativo al sistema de progresión.

- [ ] **Componente Interactuable/Recogible** *(a futuro — baja prioridad)*
  Cuando haya 8-10 interactuables distintos en el nivel, extraer un `InteractableComponent` / `CollectibleComponent` reutilizable (similar a HurtboxComponent). Centraliza: detección de player por grupo, prompt "pulsa E", señales de trigger. Por ahora el patrón directo (Area2D + OnBodyEntered) es suficiente. Ver: `Core/Entities/Groups.cs` para los strings de grupos.

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
- [x] Pipeline de resonancias y elevación (2026-02-19) — InventoryManager, ProgressionManager simplificado, ResonanceTrigger, ElevationItem, BonfireMenu, Bonfire
- [x] **Validado (2026-02-19):** input action `interact` (tecla E), enemigo persigue + ataca + dropea isótopos visibles + HUD actualiza moneda, colisión y apertura de BonfireMenu
- [x] **Validado (2026-02-20):** ResonanceTrigger → BonfireMenu → activar resonancia → stats suben correctamente
