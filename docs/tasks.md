# Tareas Pendientes — Rot of Time

> Backlog general de trabajo pendiente. Revisar al inicio de cada sesión.
> Para planes de implementación detallados ver `docs/plans/`.

---

## ⚠️ PIVOTE: Top-down → Side-scroller (2026-02-22)

**Decisión:** El vertical slice valida los sistemas core (combat, progression, artifacts, enemy AI). Se cierra la fase top-down y se inicia una nueva fase de transición a side-scroller estilo Blasphemous/HK.

**Motivo principal:** Pipeline de arte (3D model → spritesheet) es 4-8x más costoso en top-down (4-8 renders por animación vs 1 en side-scroller). Inviable para un solo dev.

**Sistemas que sobreviven:** Combat pipeline, Progression/Artifact/Economy managers, Skill hierarchy, Save/Load, Enemy AI architecture, Boss SoulFragment1 (refactor pendiente de física).

**Cancelado por pivote:** Level Design top-down (Floor 1-3), ensamblado del boss en top-down, Bloque E-1 a E-4.

---

## Siguiente fase: Side-scroller — post-transición

- [ ] **Ajuste de números de física** — gravity (980), jump velocity (-420), speed (200) tras playtest. Probable que necesiten tweaking.
- [ ] **Boss SoulFragment1** — refactor de física para platformer (lógica top-down pendiente de adaptar).
- [ ] **Nivel propio side-scroller** — TileMap con suelo + plataformas (el nivel top-down actual es provisional).
- [ ] **Coyote time y jump buffer** — mejoras de feel tras validar la física base.

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

- [~] **Boss E-4/E-5/E-6** — ~~ensamblado en Godot editor~~ *CANCELADO — refactor para side-scroller en nueva fase*
- [~] **Bloque E-1 a E-4: Level Design top-down** — ~~Floor 1-3 + arena del boss~~ *CANCELADO — se diseña para side-scroller en nueva fase*

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
- [x] Jerarquía Skill (Skill → ActiveSkill → ProjectileSkill): cooldowns encapsulados en ActiveSkill, AttackManagerComponent simplificado (sin timers externos ni SkillFired signal).
- [x] Renombrado spells Elevation 1 acorde al lore de carbono: FireballSkill → CarbonShellSkill, IceShardSkill → CarbonSplinterSkill.
- [x] Bloque D-1: Auditoría MetaData — cubre todo el estado necesario.
- [x] Bloque D-2: Save triggers — SaveMeta() al morir y al cerrar el juego.
- [x] Fix: SaveManager reescrito con System.IO (UTF-8 correcto, sin errores de encoding).
- [x] Fix: BasicEnemy.SpawnIsotopeDrop usa CallDeferred para AddChild (evita crash en physics callback).
- [x] Simplificación elevación: ElevationItem genérico (`elevation`), gate por 3 resonancias + item en Bonfire.
- [x] DebugOverlay: botones "+ Resonancia" y "+ Elevation item" para testing.
- [x] Boss Soul Fragment 1 — código C# (BossAttackSlot, BossAttackManager, SoulFragment1.cs, SoulFragmentStateMachine + 5 estados). Ensamblado cancelado — refactor para side-scroller en nueva fase.
- [x] **VERTICAL SLICE CERRADO (2026-02-22)** — pivote a side-scroller. Ver sección ⚠️ PIVOTE arriba.
- [x] **Transición a side-scroller (2026-02-23)** — EntityMovementComponent con gravedad/salto, PlayerStateMachine con JumpingState/FallingState, DashState horizontal, sprite flip, gravedad en BasicEnemy (IdleState/ChasingState/AttackingState), Input Map actualizado (jump→Space, dash→Shift).
