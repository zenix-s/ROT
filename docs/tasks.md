# Tareas Pendientes — Rot of Time

> Backlog general de trabajo pendiente. Revisar al inicio de cada sesión.
> Para planes de implementación detallados ver `docs/plans/`.

---

## En el cajón

### Código


- [ ] **Sistema de ataques cuerpo a cuerpo** *(a futuro — post-vertical-slice)*
  El `RockBody` fue reemplazado por proyectiles (`EnemyBolt`). Un ataque melee real (para un tipo de enemigo diferente) debería: aparecer brevemente en la dirección del enemigo, tener hitbox activa solo durante el swing, y desaparecer. No hacer hasta tener Level Design + Boss funcionando.

- [ ] **Lore: descripciones de Elevación**
  Añadir `ElevationData` Resource con nombre y descripción de lore por elevación (1-5). Mostrar en BonfireMenu al ascender. Da contexto narrativo al sistema de progresión.

- [ ] **Componente Interactuable/Recogible** *(a futuro — baja prioridad)*
  Cuando haya 8-10 interactuables distintos en el nivel, extraer un `InteractableComponent` / `CollectibleComponent` reutilizable (similar a HurtboxComponent). Centraliza: detección de player por grupo, prompt "pulsa E", señales de trigger. Por ahora el patrón directo (Area2D + OnBodyEntered) es suficiente. Ver: `Core/Entities/Groups.cs` para los strings de grupos.

- [ ] **Refactor: isótopos a InventoryManager**
  Mover isótopos de `EconomyManager` a `InventoryManager` para unificar todos los coleccionables en un solo lugar. Actualizar HUD, BonfireMenu (crafteo), y `MetaData`. No bloquea nada actualmente — hacer cuando haya tiempo.

- [ ] **Bloque D-3: Verificar Save/Load manual**
  Probar: equipar artefacto + activar resonancia → cerrar juego → reabrir → estado persistido. Y morir → volver al menú → reentrar → estado restaurado.

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
- [x] Bloque C-1 + C-2: ArtifactsMenu (Mesa de Artefactos) — equip/unequip + crafteo con isótopos. Accesible con K desde cualquier punto del juego.
- [x] Bloque D-1: Auditoría MetaData — cubre todo el estado necesario.
- [x] Bloque D-2: Save triggers — SaveMeta() al morir y al cerrar el juego.
- [x] Fix: SaveManager reescrito con System.IO (UTF-8 correcto, sin errores de encoding).
- [x] Fix: BasicEnemy.SpawnIsotopeDrop usa CallDeferred para AddChild (evita crash en physics callback).
- [x] Simplificación elevación: ElevationItem genérico (`elevation`), gate por 3 resonancias + item en Bonfire.
- [x] DebugOverlay: botones "+ Resonancia" y "+ Elevation item" para testing.
